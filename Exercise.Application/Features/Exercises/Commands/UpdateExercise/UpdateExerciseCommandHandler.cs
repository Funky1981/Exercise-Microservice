using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.UpdateExercise
{
    public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, bool>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExerciseCommandHandler(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(request.Id, cancellationToken);

            if (exercise is null)
                throw new NotFoundException(nameof(exercise), request.Id);

            exercise.Update(
                request.Name,
                request.BodyPart,
                request.TargetMuscle,
                request.Equipment,
                request.GifUrl,
                request.Description,
                request.Difficulty);

            await _exerciseRepository.UpdateAsync(exercise, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
