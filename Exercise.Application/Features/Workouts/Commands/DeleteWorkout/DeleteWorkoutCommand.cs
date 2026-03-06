using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.DeleteWorkout
{
    public class DeleteWorkoutCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public DeleteWorkoutCommand(Guid id) => Id = id;
    }
}
