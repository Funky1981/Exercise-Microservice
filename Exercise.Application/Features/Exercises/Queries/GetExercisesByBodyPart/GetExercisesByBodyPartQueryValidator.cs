using FluentValidation;

namespace Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart
{
    public class GetExercisesByBodyPartQueryValidator : AbstractValidator<GetExercisesByBodyPartQuery>
    {
        public GetExercisesByBodyPartQueryValidator()
        {
            RuleFor(x => x.BodyPart)
                .NotEmpty().WithMessage("BodyPart is required.")
                .MaximumLength(100).WithMessage("BodyPart must not exceed 100 characters.")
                .Matches(@"^[a-zA-Z\s\-]+$").WithMessage("BodyPart must contain only letters, spaces, or hyphens.");
        }
    }
}
