using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.DeleteExercise
{
    public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, bool>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteExerciseCommandHandler(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(request.Id, cancellationToken);

            if (exercise is null)
                throw new NotFoundException(nameof(exercise), request.Id);

            await _exerciseRepository.DeleteAsync(exercise, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
