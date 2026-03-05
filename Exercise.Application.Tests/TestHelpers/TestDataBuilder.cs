using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Tests.TestHelpers
{
    /// <summary>
    /// Provides pre-built domain entity instances for use across unit tests.
    /// Uses the Builder pattern to allow selective overriding of properties.
    /// </summary>
    public static class TestDataBuilder
    {
        // ?? Exercise builders ?????????????????????????????????????????????

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
    }
}
