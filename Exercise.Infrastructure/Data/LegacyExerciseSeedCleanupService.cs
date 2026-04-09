using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exercise.Infrastructure.Data
{
    public sealed class LegacyExerciseSeedCleanupService : IHostedService
    {
        private static readonly LegacySeedSignature[] LegacySeedSignatures =
        [
            new("Barbell Bench Press", "Classic horizontal press for chest and triceps.", "intermediate", "chest", "pectorals", "barbell"),
            new("Incline Dumbbell Press", "Upper-chest focused press with a longer range of motion.", "beginner", "chest", "upper chest", "dumbbell"),
            new("Cable Fly", "Chest isolation movement with constant tension.", "beginner", "chest", "pectorals", "cable"),
            new("Pull-Up", "Vertical pulling pattern for back and arms.", "intermediate", "back", "lats", "bodyweight"),
            new("Seated Cable Row", "Stable horizontal row for upper-back volume.", "beginner", "back", "mid back", "cable"),
            new("Lat Pulldown", "Machine-based vertical pull for lat development.", "beginner", "back", "lats", "machine"),
            new("Overhead Press", "Compound press for shoulders and triceps.", "intermediate", "shoulders", "delts", "barbell"),
            new("Dumbbell Lateral Raise", "Isolation lift for shoulder width.", "beginner", "shoulders", "side delts", "dumbbell"),
            new("Barbell Curl", "Direct biceps work with a fixed path.", "beginner", "upper arms", "biceps", "barbell"),
            new("Cable Triceps Pushdown", "Controlled triceps isolation movement.", "beginner", "upper arms", "triceps", "cable"),
            new("Hammer Curl", "Neutral-grip curl for arms and forearms.", "beginner", "lower arms", "brachialis", "dumbbell"),
            new("Neck Flexion", "Neck strengthening movement with controlled resistance.", "advanced", "neck", "neck flexors", "machine"),
            new("Back Squat", "Primary lower-body strength movement.", "intermediate", "upper legs", "quadriceps", "barbell"),
            new("Romanian Deadlift", "Hip hinge for posterior-chain strength.", "intermediate", "upper legs", "hamstrings", "barbell"),
            new("Walking Lunge", "Single-leg pattern for quads and glutes.", "beginner", "upper legs", "glutes", "dumbbell"),
            new("Standing Calf Raise", "Simple calf-focused exercise with full stretch.", "beginner", "lower legs", "calves", "machine"),
            new("Seated Calf Raise", "Bent-knee calf variation for soleus emphasis.", "beginner", "lower legs", "calves", "machine"),
            new("Hanging Knee Raise", "Anterior core movement with a controlled pelvis tuck.", "beginner", "waist", "abs", "bodyweight"),
            new("Cable Wood Chop", "Rotational core exercise with adjustable resistance.", "beginner", "waist", "obliques", "cable"),
            new("Treadmill Run", "Steady-state or interval running indoors.", "beginner", "cardio", "cardiovascular system", "treadmill"),
            new("Assault Bike Intervals", "High-output interval conditioning option.", "intermediate", "cardio", "cardiovascular system", "air bike"),
            new("Burpee", "Conditioning movement combining squat, plank, and jump.", "beginner", "cardio", "full body", "bodyweight"),
            new("Farmer Carry", "Loaded carry for trunk stiffness and grip endurance.", "intermediate", "other", "grip", "dumbbell"),
            new("Sled Push", "Low-skill conditioning and force-production drill.", "intermediate", "other", "full body", "sled"),
        ];

        private readonly IServiceScopeFactory _scopeFactory;

        public LegacyExerciseSeedCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ExerciseDbContext>();

            var exercises = await dbContext.Exercises
                .Where(exercise => exercise.Description != null && exercise.Difficulty != null)
                .ToListAsync(cancellationToken);

            var matches = exercises.Where(IsLegacySeedExercise).ToList();
            if (matches.Count == 0)
            {
                return;
            }

            dbContext.Exercises.RemoveRange(matches);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static bool IsLegacySeedExercise(Domain.Entities.Exercise exercise)
        {
            return LegacySeedSignatures.Any(signature =>
                exercise.Name == signature.Name &&
                exercise.Description == signature.Description &&
                exercise.Difficulty == signature.Difficulty &&
                exercise.BodyPart == signature.BodyPart &&
                exercise.TargetMuscle == signature.TargetMuscle &&
                (exercise.Equipment ?? string.Empty) == signature.Equipment);
        }

        private sealed record LegacySeedSignature(
            string Name,
            string Description,
            string Difficulty,
            string BodyPart,
            string TargetMuscle,
            string Equipment);
    }
}
