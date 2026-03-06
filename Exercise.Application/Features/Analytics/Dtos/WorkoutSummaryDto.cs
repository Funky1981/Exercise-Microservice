namespace Exercise.Application.Features.Analytics.Dtos
{
    public class WorkoutSummaryDto
    {
        public Guid UserId { get; set; }
        public int TotalWorkouts { get; set; }
        public int CompletedWorkouts { get; set; }
        public TimeSpan TotalWorkoutDuration { get; set; }
        public int TotalExerciseLogs { get; set; }
        public int CompletedExerciseLogs { get; set; }
        public TimeSpan TotalExerciseLogDuration { get; set; }
    }
}
