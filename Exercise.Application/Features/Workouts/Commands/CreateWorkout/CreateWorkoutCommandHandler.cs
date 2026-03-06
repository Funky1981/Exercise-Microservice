using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.CreateWorkout
{
    public class CreateWorkoutCommandHandler : IRequestHandler<CreateWorkoutCommand, Guid>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkoutCommandHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = new Workout(Guid.NewGuid(), request.UserId, request.Name, request.Date);
            workout.UpdateNotes(request.Notes);

            await _workoutRepository.AddAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return workout.Id;
        }
    }
}
