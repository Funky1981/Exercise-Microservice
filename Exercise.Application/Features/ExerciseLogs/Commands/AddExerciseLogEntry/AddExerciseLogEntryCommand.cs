using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.AddExerciseLogEntry
{
    public class AddExerciseLogEntryCommand : IRequest<bool>
    {
        public Guid LogId { get; set; }
        public Guid ExerciseId { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
