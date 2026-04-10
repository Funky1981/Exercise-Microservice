using MediatR;

namespace Exercise.Application.Features.Sessions.Commands.StartSession
{
    public class StartSessionCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public Guid WorkoutId { get; set; }
    }
}
