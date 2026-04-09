namespace Exercise.Application.Exercises.Dtos
{
    public class ExerciseFiltersDto
    {
        public IReadOnlyList<string> Regions { get; set; } = [];
        public IDictionary<string, IReadOnlyList<string>> BodyPartsByRegion { get; set; } =
            new Dictionary<string, IReadOnlyList<string>>();
        public IReadOnlyList<string> Equipment { get; set; } = [];
    }
}
