using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.WorkoutPlans.Dtos;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlanById
{
    public class GetWorkoutPlanByIdQueryHandler : IRequestHandler<GetWorkoutPlanByIdQuery, WorkoutPlanDto?>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IMapper _mapper;

        public GetWorkoutPlanByIdQueryHandler(IWorkoutPlanRepository workoutPlanRepository, IMapper mapper)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _mapper = mapper;
        }

        public async Task<WorkoutPlanDto?> Handle(GetWorkoutPlanByIdQuery request, CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanRepository.GetByIdAsync(request.Id, cancellationToken);
            return plan is null ? null : _mapper.Map<WorkoutPlanDto>(plan);
        }
    }
}
