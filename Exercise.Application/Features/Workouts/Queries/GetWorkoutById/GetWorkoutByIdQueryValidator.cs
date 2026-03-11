using FluentValidation;

namespace Exercise.Application.Features.Workouts.Queries.GetWorkoutById
{
    public class GetWorkoutByIdQueryValidator : AbstractValidator<GetWorkoutByIdQuery>
    {
        public GetWorkoutByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Workout Id must not be empty.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId is required.");
        }
    }
}
