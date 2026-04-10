namespace Exercise.Application.Features.Analytics.Dtos
{
    public class ExerciseAnalyticsDto
    {
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public int TotalSets { get; set; }
        public int TotalReps { get; set; }
        /// <summary>Volume = sum of (sets × reps) across all logged entries.</summary>
        public int TotalVolume { get; set; }
        public double AvgRepsPerSet { get; set; }
        public double AvgRestSeconds { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public List<ExerciseDataPoint> DataPoints { get; set; } = new();
    }

    public class ExerciseDataPoint
    {
        public DateTime Date { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int Volume { get; set; }
        public double DurationSeconds { get; set; }
        public double RestSeconds { get; set; }
    }
}
