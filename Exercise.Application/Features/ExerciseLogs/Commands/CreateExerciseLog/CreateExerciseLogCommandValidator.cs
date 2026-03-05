using FluentValidation;

namespace Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog
{
    public class CreateExerciseLogCommandValidator : AbstractValidator<CreateExerciseLogCommand>
    {
        public CreateExerciseLogCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.");

            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
                .When(x => x.Notes is not null);
        }
    }
}
