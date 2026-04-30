namespace Exercise.Application.Exercises.Dtos
{
    public class ExerciseMediaReviewQueueItemDto
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string ExerciseBodyPart { get; set; } = string.Empty;
        public string ExerciseTargetMuscle { get; set; } = string.Empty;
        public string? ExerciseEquipment { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string? MediaKind { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? SourcePageUrl { get; set; }
        public string SourceProvider { get; set; } = string.Empty;
        public string? SourceTitle { get; set; }
        public decimal MatchScore { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}