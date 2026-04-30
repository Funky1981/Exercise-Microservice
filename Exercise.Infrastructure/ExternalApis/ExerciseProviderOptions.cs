namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class ExerciseProviderOptions
    {
        public const string SectionName = "ExerciseProviders";

        public string[] CatalogProviders { get; set; } = ["RapidApi", "Wger"];

        public string[] MediaProviders { get; set; } = ["Wger", "Pexels", "WikimediaCommons", "Openverse"];

        public int MediaExerciseLimit { get; set; } = 250;

        public double MediaCandidateMinimumScore { get; set; } = 0.45d;

        public double MediaAutoAcceptScore { get; set; } = 0.80d;
    }
}
