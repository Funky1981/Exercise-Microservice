using FluentValidation;

namespace Exercise.Application.Features.Exercises.Commands.UpdateExercise
{
    public class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
    {
        public UpdateExerciseCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Exercise Id must not be empty.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.BodyPart)
                .NotEmpty().WithMessage("BodyPart is required.")
                .MaximumLength(100).WithMessage("BodyPart must not exceed 100 characters.");

            RuleFor(x => x.TargetMuscle)
                .NotEmpty().WithMessage("TargetMuscle is required.")
                .MaximumLength(100).WithMessage("TargetMuscle must not exceed 100 characters.");

            RuleFor(x => x.Equipment)
                .MaximumLength(100).WithMessage("Equipment must not exceed 100 characters.")
                .When(x => x.Equipment is not null);

            RuleFor(x => x.GifUrl)
                .MaximumLength(500).WithMessage("GifUrl must not exceed 500 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("GifUrl must be a valid URL.")
                .When(x => !string.IsNullOrWhiteSpace(x.GifUrl));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Difficulty)
                .MaximumLength(50).WithMessage("Difficulty must not exceed 50 characters.")
                .When(x => x.Difficulty is not null);
        }
    }
}
