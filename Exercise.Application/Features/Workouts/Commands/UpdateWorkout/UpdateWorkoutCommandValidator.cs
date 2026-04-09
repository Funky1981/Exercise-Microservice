using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.UpdateWorkout
{
    public class UpdateWorkoutCommandValidator : AbstractValidator<UpdateWorkoutCommand>
    {
        public UpdateWorkoutCommandValidator()
        {
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId is required.");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
            RuleFor(x => x.ExerciseIds)
                .NotEmpty()
                .WithMessage("Select at least one exercise.");
        }
    }
}
