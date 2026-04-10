using MediatR;

namespace Exercise.Application.Features.Sessions.Commands.LogSet
{
    public class LogSetCommand : IRequest<bool>
    {
        public Guid LogId { get; set; }
        public Guid CurrentUserId { get; set; }
        public Guid ExerciseId { get; set; }
        public int Reps { get; set; }
        public int DurationSeconds { get; set; }
        public int RestSeconds { get; set; }
    }
}
