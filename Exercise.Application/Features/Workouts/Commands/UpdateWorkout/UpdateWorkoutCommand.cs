using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.UpdateWorkout
{
    public class UpdateWorkoutCommand : IRequest<bool>
    {
        public Guid WorkoutId { get; set; }
        public Guid CurrentUserId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
