using Exercise.Application.Abstractions.Repositories;
using MediatR;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Features.Exercises.Commands.CreateExercise
{
    public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, Guid>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateExerciseCommandHandler(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = new ExerciseEntity(
                Guid.NewGuid(),
                request.Name,
                request.BodyPart,
                request.TargetMuscle,
                request.Equipment,
                request.GifUrl,
                request.Description,
                request.Difficulty);

            await _exerciseRepository.AddAsync(exercise, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return exercise.Id;
        }
    }
}
