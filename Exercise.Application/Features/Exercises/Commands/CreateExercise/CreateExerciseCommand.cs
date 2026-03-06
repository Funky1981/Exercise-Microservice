using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.CreateExercise
{
    public class CreateExerciseCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public string TargetMuscle { get; set; } = string.Empty;
        public string? Equipment { get; set; }
        public string? GifUrl { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
    }
}
