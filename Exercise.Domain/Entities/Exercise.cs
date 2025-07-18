namespace Exercise.Domain.Entities
{
    public class Exercise
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string BodyPart { get; set; }
        public string? Equipment { get; set; }
        public string TargetMuscle { get; set; }
        public string? GifUrl { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
    }
}
