using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Sessions.Commands.LogSet
{
    public class LogSetCommandHandler : IRequestHandler<LogSetCommand, bool>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LogSetCommandHandler(IExerciseLogRepository exerciseLogRepository, IUnitOfWork unitOfWork)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(LogSetCommand request, CancellationToken cancellationToken)
        {
            var log = await _exerciseLogRepository.GetOwnedByIdForUpdateAsync(
                request.LogId, request.CurrentUserId, cancellationToken);

            if (log is null)
            {
                if (await _exerciseLogRepository.ExistsAsync(request.LogId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this exercise log.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.ExerciseLog), request.LogId);
            }

            log.LogSet(
                request.ExerciseId,
                request.Reps,
                request.DurationSeconds > 0 ? TimeSpan.FromSeconds(request.DurationSeconds) : null,
                request.RestSeconds > 0 ? TimeSpan.FromSeconds(request.RestSeconds) : null,
                DateTime.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
