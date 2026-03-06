using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.CompleteExerciseLog
{
    public class CompleteExerciseLogCommand : IRequest<bool>
    {
        public Guid LogId { get; set; }
        public TimeSpan? TotalDuration { get; set; }
    }
}
