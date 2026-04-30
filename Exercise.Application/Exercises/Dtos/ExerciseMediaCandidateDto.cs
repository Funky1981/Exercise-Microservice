namespace Exercise.Application.Exercises.Dtos
{
    public class ExerciseMediaCandidateDto
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string? MediaKind { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? SourcePageUrl { get; set; }
        public string SourceProvider { get; set; } = string.Empty;
        public string? SourcePayloadJson { get; set; }
        public string? SourceTitle { get; set; }
        public decimal MatchScore { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public string? ReviewNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public bool IsSelected { get; set; }
    }
}