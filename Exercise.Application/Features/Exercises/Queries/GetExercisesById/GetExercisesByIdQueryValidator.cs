using FluentValidation;

namespace Exercise.Application.Features.Exercises.Queries.GetExercisesById
{
    public class GetExercisesByIdQueryValidator : AbstractValidator<GetExercisesByIdQuery>
    {
        public GetExercisesByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Exercise Id must not be empty.");
        }
    }
}
