using FluentValidation;

namespace Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogById
{
    public class GetExerciseLogByIdQueryValidator : AbstractValidator<GetExerciseLogByIdQuery>
    {
        public GetExerciseLogByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ExerciseLog Id is required.");
            RuleFor(x => x.CurrentUserId)
                .NotEmpty().WithMessage("CurrentUserId is required.");
        }
    }
}
