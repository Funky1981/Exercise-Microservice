namespace Exercise.Application.Features.WorkoutPlans.Dtos
{
    public class WorkoutPlanWorkoutDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Duration { get; set; }
        public bool IsCompleted { get; set; }
        public IReadOnlyList<Guid> ExerciseIds { get; set; } = [];
    }

    public class WorkoutPlanDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyList<WorkoutPlanWorkoutDto> Workouts { get; set; } = [];
    }
}
