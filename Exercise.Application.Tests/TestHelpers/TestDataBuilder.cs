using Exercise.Domain.Entities;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Tests.TestHelpers
{
    /// <summary>
    /// Provides pre-built domain entity instances for use across unit tests.
    /// Uses the Builder pattern to allow selective overriding of properties.
    /// </summary>
    public static class TestDataBuilder
    {
        // Exercise builders

        /// <summary>Returns a valid Exercise entity with sensible defaults.</summary>
        public static ExerciseEntity BuildExercise(
            Guid?   id            = null,
            string  name          = "Push Up",
            string  bodyPart      = "Chest",
            string  targetMuscle  = "Pectorals",
            string? equipment     = "None",
            string? gifUrl        = "http://example.com/pushup.gif",
            string? description   = "A basic push-up exercise.",
            string? difficulty    = "Medium")
        {
            return new ExerciseEntity(
                id           ?? Guid.NewGuid(),
                name,
                bodyPart,
                targetMuscle,
                equipment,
                gifUrl,
                description,
                difficulty);
        }

        /// <summary>Returns a list of exercises, all for the same body part.</summary>
        public static List<ExerciseEntity> BuildExerciseList(string bodyPart = "Chest", int count = 2)
        {
            return Enumerable.Range(1, count).Select(i => BuildExercise(
                name:         $"Exercise {i}",
                bodyPart:     bodyPart,
                targetMuscle: "Pectorals"
            )).ToList();
        }

        /// <summary>Returns a mixed list spanning multiple body parts.</summary>
        public static List<ExerciseEntity> BuildMixedExerciseList() =>
        [
            BuildExercise(name: "Push Up",    bodyPart: "Chest", targetMuscle: "Pectorals"),
            BuildExercise(name: "Squat",      bodyPart: "Legs",  targetMuscle: "Quadriceps", equipment: "Barbell"),
            BuildExercise(name: "Pull Up",    bodyPart: "Back",  targetMuscle: "Lats",       equipment: "Pull-up bar"),
            BuildExercise(name: "Plank",      bodyPart: "Core",  targetMuscle: "Abs",        equipment: null),
        ];

        // User builders

        public static User BuildUser(
            Guid?   id       = null,
            string  name     = "Test User",
            string  email    = "test@example.com",
            string? provider = null)
        {
            return new User(id ?? Guid.NewGuid(), name, email, provider);
        }

        // Workout builders

        public static Workout BuildWorkout(
            Guid?     id       = null,
            Guid?     userId   = null,
            string?   name     = "Morning Session",
            DateTime? date     = null)
        {
            return new Workout(
                id     ?? Guid.NewGuid(),
                userId ?? Guid.NewGuid(),
                name,
                date   ?? DateTime.UtcNow);
        }

        public static List<Workout> BuildWorkoutList(Guid userId, int count = 3)
        {
            return Enumerable.Range(1, count)
                .Select(i => BuildWorkout(userId: userId, name: $"Workout {i}",
                    date: DateTime.UtcNow.AddDays(-i)))
                .ToList();
        }

        // WorkoutPlan builders

        public static WorkoutPlan BuildWorkoutPlan(
            Guid?     id        = null,
            Guid?     userId    = null,
            string?   name      = "12-Week Plan",
            DateTime? startDate = null,
            DateTime? endDate   = null)
        {
            return new WorkoutPlan(
                id        ?? Guid.NewGuid(),
                userId    ?? Guid.NewGuid(),
                name,
                startDate ?? DateTime.UtcNow,
                endDate);
        }

        // ExerciseLog builders

        public static ExerciseLog BuildExerciseLog(
            Guid?     id     = null,
            Guid?     userId = null,
            string?   name   = "Monday Log",
            DateTime? date   = null)
        {
            return new ExerciseLog(
                id     ?? Guid.NewGuid(),
                userId ?? Guid.NewGuid(),
                name,
                date   ?? DateTime.UtcNow);
        }
    }
}
