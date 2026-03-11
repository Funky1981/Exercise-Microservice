using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Application.Features.Workouts.Dtos;
using MediatR;

namespace Exercise.Application.Features.Workouts.Queries.GetWorkoutById
{
    public class GetWorkoutByIdQueryHandler : IRequestHandler<GetWorkoutByIdQuery, WorkoutDto?>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IMapper _mapper;

        public GetWorkoutByIdQueryHandler(IWorkoutRepository workoutRepository, IMapper mapper)
        {
            _workoutRepository = workoutRepository;
            _mapper = mapper;
        }

        public async Task<WorkoutDto?> Handle(GetWorkoutByIdQuery request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetOwnedByIdAsync(request.Id, request.CurrentUserId, cancellationToken);
            if (workout is null)
            {
                if (await _workoutRepository.ExistsAsync(request.Id, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.Workout), request.Id);
            }

            return _mapper.Map<WorkoutDto>(workout);
        }
    }
}
