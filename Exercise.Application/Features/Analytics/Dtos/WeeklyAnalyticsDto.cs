namespace Exercise.Application.Features.Analytics.Dtos
{
    public class WeeklyAnalyticsDto
    {
        public List<WeeklyDataPoint> Weeks { get; set; } = new();
        public double AvgWorkoutsPerWeek { get; set; }
        public int TotalVolume { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double AvgRestSeconds { get; set; }
    }

    public class WeeklyDataPoint
    {
        public DateTime WeekStart { get; set; }
        public int WorkoutCount { get; set; }
        public int TotalSets { get; set; }
        public int TotalReps { get; set; }
        public int Volume { get; set; }
        public double DurationSeconds { get; set; }
        public double AvgRestSeconds { get; set; }
    }
}
