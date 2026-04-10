using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.Sessions.Commands.StartSession
{
    public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, Guid>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StartSessionCommandHandler(
            IWorkoutRepository workoutRepository,
            IExerciseLogRepository exerciseLogRepository,
            IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _exerciseLogRepository = exerciseLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(StartSessionCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetOwnedByIdWithExercisesAsync(
                request.WorkoutId, request.UserId, cancellationToken);

            if (workout is null)
            {
                if (await _workoutRepository.ExistsAsync(request.WorkoutId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout.");

                throw new NotFoundException(nameof(Workout), request.WorkoutId);
            }

            var log = new ExerciseLog(
                Guid.NewGuid(),
                request.UserId,
                workout.Name,
                DateTime.UtcNow,
                workout.Id);

            await _exerciseLogRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return log.Id;
        }
    }
}
