using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.WorkoutPlans.Dtos;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlansByUserId
{
    public class GetWorkoutPlansByUserIdQueryHandler
        : IRequestHandler<GetWorkoutPlansByUserIdQuery, PagedResult<WorkoutPlanDto>>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IMapper _mapper;

        public GetWorkoutPlansByUserIdQueryHandler(IWorkoutPlanRepository workoutPlanRepository, IMapper mapper)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<WorkoutPlanDto>> Handle(
            GetWorkoutPlansByUserIdQuery request,
            CancellationToken cancellationToken)
        {
            var plans = await _workoutPlanRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            var dtos = _mapper.Map<List<WorkoutPlanDto>>(plans);

            var paged = dtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<WorkoutPlanDto>(paged, dtos.Count, request.PageNumber, request.PageSize);
        }
    }
}
