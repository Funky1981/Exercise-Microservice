using FluentValidation;

namespace Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog
{
    public class DeleteExerciseLogCommandValidator : AbstractValidator<DeleteExerciseLogCommand>
    {
        public DeleteExerciseLogCommandValidator()
        {
            RuleFor(x => x.LogId).NotEmpty();
        }
    }
}
