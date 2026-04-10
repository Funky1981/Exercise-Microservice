using System;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class WorkoutExercise
    {
        public Guid Id { get; private set; }
        public Guid WorkoutId { get; private set; }
        public Guid ExerciseId { get; private set; }
        public Exercise Exercise { get; private set; } = null!;
        public int Sets { get; private set; }
        public int Reps { get; private set; }
        public int RestSeconds { get; private set; }
        public int Order { get; private set; }

        private WorkoutExercise() { } // For EF Core

        public WorkoutExercise(Guid id, Guid workoutId, Guid exerciseId, Exercise exercise, int order,
            int sets = 3, int reps = 10, int restSeconds = 60)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(workoutId, nameof(workoutId));
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
            Guard.AgainstNull(exercise, nameof(exercise));

            Id = id;
            WorkoutId = workoutId;
            ExerciseId = exerciseId;
            Exercise = exercise;
            Order = order;
            Sets = sets;
            Reps = reps;
            RestSeconds = restSeconds;
        }

        public void UpdatePrescription(int sets, int reps, int restSeconds)
        {
            Sets = sets;
            Reps = reps;
            RestSeconds = restSeconds;
        }

        public void UpdateOrder(int order)
        {
            Order = order;
        }
    }
}
