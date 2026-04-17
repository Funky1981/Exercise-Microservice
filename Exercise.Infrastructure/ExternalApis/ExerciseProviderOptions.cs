namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class ExerciseProviderOptions
    {
        public const string SectionName = "ExerciseProvider";

        public string Provider { get; set; } = "RapidApi";
    }
}
