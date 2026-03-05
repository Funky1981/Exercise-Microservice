using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog
{
    public class CreateExerciseLogCommandHandler : IRequestHandler<CreateExerciseLogCommand, Guid>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateExerciseLogCommandHandler(IExerciseLogRepository exerciseLogRepository, IUnitOfWork unitOfWork)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateExerciseLogCommand request, CancellationToken cancellationToken)
        {
            var log = new ExerciseLog(Guid.NewGuid(), request.UserId, request.Name, request.Date);
            log.UpdateNotes(request.Notes);

            await _exerciseLogRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return log.Id;
        }
    }
}
