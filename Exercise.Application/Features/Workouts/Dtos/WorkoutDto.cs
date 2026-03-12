namespace Exercise.Application.Features.Workouts.Dtos
{
    public class WorkoutExerciseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public string TargetMuscle { get; set; } = string.Empty;
        public string? Equipment { get; set; }
    }

    public class WorkoutDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; }
        public IReadOnlyList<WorkoutExerciseDto> Exercises { get; set; } = [];
    }
}
