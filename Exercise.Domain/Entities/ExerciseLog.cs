using System;
using System.Collections.Generic;
using System.Linq;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class ExerciseLog
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string? Name { get; private set; }
        
        private readonly List<ExerciseLogEntry> _exercisesCompleted = new();
        public IReadOnlyList<ExerciseLogEntry> ExercisesCompleted => _exercisesCompleted.AsReadOnly();
        
        public DateTime Date { get; private set; }
        public TimeSpan? Duration { get; private set; }
        public string? Notes { get; private set; }
        public bool IsCompleted { get; private set; }

        private ExerciseLog() { } // For EF Core

        public ExerciseLog(Guid id, Guid userId, string? name, DateTime date)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(userId, nameof(userId));

            Id = id;
            UserId = userId;
            Name = name;
            Date = date;
            IsCompleted = false;
        }

        public void AddEntry(Guid exerciseId, int sets, int reps, TimeSpan? duration = null)
        {
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
            Guard.AgainstNegativeOrZero(sets, nameof(sets));
            Guard.AgainstNegativeOrZero(reps, nameof(reps));

            if (IsCompleted)
                throw new InvalidOperationException("Cannot add entries to a completed log.");

            var entry = new ExerciseLogEntry(exerciseId, sets, reps, duration);
            _exercisesCompleted.Add(entry);
        }

        public void CompleteLog(TimeSpan? totalDuration = null)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Log is already completed.");

            Duration = totalDuration ?? CalculateTotalDuration();
            IsCompleted = true;
        }

        public TimeSpan CalculateTotalDuration()
        {
            return TimeSpan.FromSeconds(_exercisesCompleted
                .Where(e => e.Duration.HasValue)
                .Sum(e => e.Duration!.Value.TotalSeconds));
        }

        public void UpdateNotes(string? notes)
        {
            Notes = notes;
        }
    }

    public class ExerciseLogEntry
    {
        public Guid ExerciseId { get; private set; }
        public int Sets { get; private set; }
        public int Reps { get; private set; }
        public TimeSpan? Duration { get; private set; }

        private ExerciseLogEntry() { } // For EF Core

        public ExerciseLogEntry(Guid exerciseId, int sets, int reps, TimeSpan? duration = null)
        {
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
            Guard.AgainstNegativeOrZero(sets, nameof(sets));
            Guard.AgainstNegativeOrZero(reps, nameof(reps));

            ExerciseId = exerciseId;
            Sets = sets;
            Reps = reps;
            Duration = duration;
        }
    }
}
