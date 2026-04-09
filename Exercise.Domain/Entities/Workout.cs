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

        private readonly List<Exercise> _exercises = new();
        private string ExerciseOrder { get; set; } = string.Empty;
        public IReadOnlyList<Exercise> Exercises => GetOrderedExercises();

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

        public void AddExercise(Exercise exercise)
        {
            Guard.AgainstNull(exercise, nameof(exercise));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot add exercises to a completed workout.");
            
            if (_exercises.Any(e => e.Id == exercise.Id))
                return;

            _exercises.Add(exercise);
            UpdateExerciseOrder();
        }

        public void RemoveExercise(Guid exerciseId)
        {
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot remove exercises from a completed workout.");

            var exerciseToRemove = _exercises.FirstOrDefault(e => e.Id == exerciseId);
            if (exerciseToRemove != null)
            {
                _exercises.Remove(exerciseToRemove);
                UpdateExerciseOrder();
            }
        }

        public void ReplaceExercises(IEnumerable<Exercise> exercises)
        {
            Guard.AgainstNull(exercises, nameof(exercises));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot update exercises for a completed workout.");

            _exercises.Clear();
            foreach (var exercise in exercises.DistinctBy(exercise => exercise.Id))
            {
                _exercises.Add(exercise);
            }

            UpdateExerciseOrder();
        }

        public void CompleteWorkout(TimeSpan duration)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Workout is already completed.");

            Guard.AgainstNegativeOrZero(duration, nameof(duration));

            Duration = duration;
            IsCompleted = true;
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

        private IReadOnlyList<Exercise> GetOrderedExercises()
        {
            if (_exercises.Count <= 1 || string.IsNullOrWhiteSpace(ExerciseOrder))
            {
                return _exercises.AsReadOnly();
            }

            var lookup = _exercises.ToDictionary(exercise => exercise.Id);
            var ordered = new List<Exercise>(_exercises.Count);

            foreach (var id in ExerciseOrder
                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(raw => Guid.TryParse(raw, out var value) ? value : Guid.Empty)
                         .Where(value => value != Guid.Empty))
            {
                if (lookup.Remove(id, out var exercise))
                {
                    ordered.Add(exercise);
                }
            }

            ordered.AddRange(lookup.Values.OrderBy(exercise => exercise.Name));
            return ordered.AsReadOnly();
        }

        private void UpdateExerciseOrder()
        {
            ExerciseOrder = string.Join(',', _exercises.Select(exercise => exercise.Id));
        }
    }
}
