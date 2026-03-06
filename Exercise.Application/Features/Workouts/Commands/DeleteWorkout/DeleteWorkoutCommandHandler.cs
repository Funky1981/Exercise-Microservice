using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.DeleteWorkout
{
    public class DeleteWorkoutCommandHandler : IRequestHandler<DeleteWorkoutCommand, bool>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteWorkoutCommandHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetByIdAsync(request.Id, cancellationToken);
            if (workout is null)
                throw new NotFoundException(nameof(workout), request.Id);

            await _workoutRepository.DeleteAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
