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
        public IReadOnlyList<Exercise> Exercises => _exercises.AsReadOnly();

        public DateTime Date { get; private set; }
        public TimeSpan? Duration { get; private set; }
        public string? Notes { get; private set; }
        public bool IsCompleted { get; private set; }

        private Workout() { } // For ORM

        public Workout(Guid id, Guid userId, string? name, DateTime date)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(userId, nameof(userId));

            Id = id;
            UserId = userId;
            Name = name;
            Date = date;
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

        public void UpdateNotes(string? notes)
        {
            Notes = notes;
        }
    }
}
