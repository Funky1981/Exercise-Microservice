using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.DeleteWorkout
{
    public class DeleteWorkoutCommandValidator : AbstractValidator<DeleteWorkoutCommand>
    {
        public DeleteWorkoutCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Workout Id must not be empty.");
        }
    }
}
