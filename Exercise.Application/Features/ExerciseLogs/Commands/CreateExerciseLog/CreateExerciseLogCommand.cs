using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog
{
    public class CreateExerciseLogCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
