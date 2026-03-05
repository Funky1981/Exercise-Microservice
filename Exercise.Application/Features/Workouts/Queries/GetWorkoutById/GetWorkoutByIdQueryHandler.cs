using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
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
            var workout = await _workoutRepository.GetByIdAsync(request.Id, cancellationToken);
            return workout is null ? null : _mapper.Map<WorkoutDto>(workout);
        }
    }
}
