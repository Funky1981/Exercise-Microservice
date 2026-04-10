using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Sessions.Commands.EndSession
{
    public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, bool>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EndSessionCommandHandler(IExerciseLogRepository exerciseLogRepository, IUnitOfWork unitOfWork)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(EndSessionCommand request, CancellationToken cancellationToken)
        {
            var log = await _exerciseLogRepository.GetOwnedByIdForUpdateAsync(
                request.LogId, request.CurrentUserId, cancellationToken);

            if (log is null)
            {
                if (await _exerciseLogRepository.ExistsAsync(request.LogId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this exercise log.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.ExerciseLog), request.LogId);
            }

            var totalDuration = request.TotalDurationSeconds > 0
                ? TimeSpan.FromSeconds(request.TotalDurationSeconds)
                : (TimeSpan?)null;

            log.CompleteLog(totalDuration);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
