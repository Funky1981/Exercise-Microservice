using MediatR;

namespace Exercise.Application.Features.Sessions.Commands.EndSession
{
    public class EndSessionCommand : IRequest<bool>
    {
        public Guid LogId { get; set; }
        public Guid CurrentUserId { get; set; }
        public int TotalDurationSeconds { get; set; }
    }
}
