using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.CreateWorkout
{
    public class CreateWorkoutCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
