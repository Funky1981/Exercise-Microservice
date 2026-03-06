using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.AddExerciseLogEntry
{
    public class AddExerciseLogEntryCommandHandler : IRequestHandler<AddExerciseLogEntryCommand, bool>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddExerciseLogEntryCommandHandler(IExerciseLogRepository exerciseLogRepository, IUnitOfWork unitOfWork)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddExerciseLogEntryCommand request, CancellationToken cancellationToken)
        {
            var log = await _exerciseLogRepository.GetByIdAsync(request.LogId, cancellationToken);
            if (log is null)
                throw new NotFoundException(nameof(Domain.Entities.ExerciseLog), request.LogId);

            log.AddEntry(request.ExerciseId, request.Sets, request.Reps, request.Duration);

            await _exerciseLogRepository.UpdateAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
