namespace Exercise.Application.Exercises.Dtos
{
    public class ExerciseDto
    {
        public Guid Id { get; set; }
        public string? ExternalId { get; set; }
        public string? SourceProvider { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public string? Equipment { get; set; }
        public string TargetMuscle { get; set; } = string.Empty;
        public string? GifUrl { get; set; }
        public IReadOnlyList<string> SecondaryMuscles { get; set; } = [];
        public IReadOnlyList<string> Instructions { get; set; } = [];
        public string? SourcePayloadJson { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public string? Category { get; set; }
    }
}