namespace Exercise.Application.Features.Workouts.Dtos
{
    public class WorkoutExerciseDto
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public string TargetMuscle { get; set; } = string.Empty;
        public string? Equipment { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int RestSeconds { get; set; }
        public int Order { get; set; }
    }

    public class WorkoutDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public bool HasExplicitTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; }
        public IReadOnlyList<WorkoutExerciseDto> Exercises { get; set; } = [];
    }
}
