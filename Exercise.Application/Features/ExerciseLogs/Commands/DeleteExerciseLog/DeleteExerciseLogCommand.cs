using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog
{
    public class DeleteExerciseLogCommand : IRequest<bool>
    {
        public Guid LogId { get; set; }
        public Guid CurrentUserId { get; set; }
        public DeleteExerciseLogCommand(Guid logId) => LogId = logId;
    }
}
