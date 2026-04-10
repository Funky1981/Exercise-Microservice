namespace Exercise.Application.Features.ExerciseLogs.Dtos
{
    public class ExerciseLogEntryDto
    {
        public Guid ExerciseId { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public TimeSpan? Duration { get; set; }
        public TimeSpan? RestTime { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class ExerciseLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? WorkoutId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; }
        public IReadOnlyList<ExerciseLogEntryDto> Entries { get; set; } = [];
    }
}
