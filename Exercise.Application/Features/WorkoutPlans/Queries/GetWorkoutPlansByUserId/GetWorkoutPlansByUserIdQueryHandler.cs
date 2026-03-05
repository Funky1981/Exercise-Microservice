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
            var skip = (request.PageNumber - 1) * request.PageSize;
            var (plans, totalCount) = await _workoutPlanRepository.GetPagedByUserIdAsync(
                request.UserId, skip, request.PageSize, cancellationToken);

            var dtos = _mapper.Map<List<WorkoutPlanDto>>(plans);
            return new PagedResult<WorkoutPlanDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
