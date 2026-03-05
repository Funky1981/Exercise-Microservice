using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog
{
    public class DeleteExerciseLogCommandHandler : IRequestHandler<DeleteExerciseLogCommand, bool>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteExerciseLogCommandHandler(IExerciseLogRepository exerciseLogRepository, IUnitOfWork unitOfWork)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteExerciseLogCommand request, CancellationToken cancellationToken)
        {
            var log = await _exerciseLogRepository.GetByIdAsync(request.LogId, cancellationToken);
            if (log is null)
                throw new NotFoundException(nameof(Domain.Entities.ExerciseLog), request.LogId);

            await _exerciseLogRepository.DeleteAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
