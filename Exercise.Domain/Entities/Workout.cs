using System;
using System.Collections.Generic;
using System.Linq;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class Workout
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string? Name { get; private set; }

        private readonly List<WorkoutExercise> _workoutExercises = new();
        public IReadOnlyList<WorkoutExercise> WorkoutExercises =>
            _workoutExercises.OrderBy(we => we.Order).ToList().AsReadOnly();

        public DateTime Date { get; private set; }
        public bool HasExplicitTime { get; private set; }
        public TimeSpan? Duration { get; private set; }
        public string? Notes { get; private set; }
        public bool IsCompleted { get; private set; }

        private Workout() { } // For ORM

        public Workout(Guid id, Guid userId, string? name, DateTime date, bool hasExplicitTime)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(userId, nameof(userId));

            Id = id;
            UserId = userId;
            Name = name;
            Date = date;
            HasExplicitTime = hasExplicitTime;
            IsCompleted = false;
        }

        public void AddExercise(Exercise exercise, int sets = 3, int reps = 10, int restSeconds = 60)
        {
            Guard.AgainstNull(exercise, nameof(exercise));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot add exercises to a completed workout.");

            if (_workoutExercises.Any(we => we.ExerciseId == exercise.Id))
                return;

            var order = _workoutExercises.Count;
            _workoutExercises.Add(new WorkoutExercise(
                Guid.NewGuid(), Id, exercise.Id, exercise, order, sets, reps, restSeconds));
        }

        public void RemoveExercise(Guid exerciseId)
        {
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot remove exercises from a completed workout.");

            var toRemove = _workoutExercises.FirstOrDefault(we => we.ExerciseId == exerciseId);
            if (toRemove != null)
            {
                _workoutExercises.Remove(toRemove);
                ReorderExercises();
            }
        }

        public void ReplaceExercises(IEnumerable<Exercise> exercises)
        {
            Guard.AgainstNull(exercises, nameof(exercises));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot update exercises for a completed workout.");

            _workoutExercises.Clear();
            int order = 0;
            foreach (var exercise in exercises.DistinctBy(e => e.Id))
            {
                _workoutExercises.Add(new WorkoutExercise(
                    Guid.NewGuid(), Id, exercise.Id, exercise, order++));
            }
        }

        public void CompleteWorkout(TimeSpan duration)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Workout is already completed.");

            Guard.AgainstNegativeOrZero(duration, nameof(duration));

            Duration = duration;
            IsCompleted = true;
        }

        public void UpdateExercisePrescription(Guid exerciseId, int sets, int reps, int restSeconds)
        {
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot update exercises on a completed workout.");

            var we = _workoutExercises.FirstOrDefault(x => x.ExerciseId == exerciseId)
                ?? throw new InvalidOperationException($"Exercise {exerciseId} is not in this workout.");

            we.UpdatePrescription(sets, reps, restSeconds);
        }

        public void UpdateNotes(string? notes)
        {
            Notes = notes;
        }

        public void Update(string? name, DateTime date, string? notes, bool hasExplicitTime)
        {
            Name = name;
            Date = date;
            Notes = notes;
            HasExplicitTime = hasExplicitTime;
        }

        private void ReorderExercises()
        {
            for (int i = 0; i < _workoutExercises.Count; i++)
            {
                _workoutExercises[i].UpdateOrder(i);
            }
        }
    }
}
