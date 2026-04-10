using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.UpdateExercisePrescription
{
    public class UpdateExercisePrescriptionCommandValidator : AbstractValidator<UpdateExercisePrescriptionCommand>
    {
        public UpdateExercisePrescriptionCommandValidator()
        {
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId is required.");
            RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("ExerciseId is required.");
            RuleFor(x => x.Sets).InclusiveBetween(1, 100).WithMessage("Sets must be between 1 and 100.");
            RuleFor(x => x.Reps).InclusiveBetween(0, 999).WithMessage("Reps must be between 0 and 999.");
            RuleFor(x => x.RestSeconds).InclusiveBetween(0, 600).WithMessage("RestSeconds must be between 0 and 600.");
        }
    }
}
