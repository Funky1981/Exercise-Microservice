using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.Workouts.Dtos;
using MediatR;

namespace Exercise.Application.Features.Workouts.Queries.GetWorkoutsByUserId
{
    public class GetWorkoutsByUserIdQueryHandler : IRequestHandler<GetWorkoutsByUserIdQuery, PagedResult<WorkoutDto>>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IMapper _mapper;

        public GetWorkoutsByUserIdQueryHandler(IWorkoutRepository workoutRepository, IMapper mapper)
        {
            _workoutRepository = workoutRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<WorkoutDto>> Handle(GetWorkoutsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var workouts = await _workoutRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            var dtos = _mapper.Map<List<WorkoutDto>>(workouts);

            var paged = dtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<WorkoutDto>(paged, dtos.Count, request.PageNumber, request.PageSize);
        }
    }
}
