using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.CompleteExerciseLog
{
    public class CompleteExerciseLogCommandHandler : IRequestHandler<CompleteExerciseLogCommand, bool>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompleteExerciseLogCommandHandler(IExerciseLogRepository exerciseLogRepository, IUnitOfWork unitOfWork)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CompleteExerciseLogCommand request, CancellationToken cancellationToken)
        {
            var log = await _exerciseLogRepository.GetByIdAsync(request.LogId, cancellationToken);
            if (log is null)
                throw new NotFoundException(nameof(Domain.Entities.ExerciseLog), request.LogId);

            log.CompleteLog(request.TotalDuration);

            await _exerciseLogRepository.UpdateAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
