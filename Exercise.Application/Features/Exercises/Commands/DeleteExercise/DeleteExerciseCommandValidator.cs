using FluentValidation;

namespace Exercise.Application.Features.Exercises.Commands.DeleteExercise
{
    public class DeleteExerciseCommandValidator : AbstractValidator<DeleteExerciseCommand>
    {
        public DeleteExerciseCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Exercise Id must not be empty.");
        }
    }
}
