using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.DeleteExercise
{
    public class DeleteExerciseCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public DeleteExerciseCommand(Guid id)
        {
            Id = id;
        }
    }
}
