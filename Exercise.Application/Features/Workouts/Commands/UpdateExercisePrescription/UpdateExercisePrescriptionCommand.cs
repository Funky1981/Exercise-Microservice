using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.UpdateExercisePrescription
{
    public class UpdateExercisePrescriptionCommand : IRequest<bool>
    {
        public Guid WorkoutId { get; set; }
        public Guid ExerciseId { get; set; }
        public Guid CurrentUserId { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int RestSeconds { get; set; }
    }
}
