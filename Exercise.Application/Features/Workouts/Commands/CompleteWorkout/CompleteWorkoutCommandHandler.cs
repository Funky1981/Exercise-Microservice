using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.CompleteWorkout
{
    public class CompleteWorkoutCommandHandler : IRequestHandler<CompleteWorkoutCommand, bool>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompleteWorkoutCommandHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetByIdAsync(request.WorkoutId, cancellationToken);
            if (workout is null)
                throw new NotFoundException(nameof(workout), request.WorkoutId);

            workout.CompleteWorkout(request.Duration);

            await _workoutRepository.UpdateAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
