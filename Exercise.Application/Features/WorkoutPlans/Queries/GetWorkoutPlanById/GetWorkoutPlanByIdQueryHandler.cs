using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
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
            var plan = await _workoutPlanRepository.GetOwnedByIdAsync(request.Id, request.CurrentUserId, cancellationToken);
            if (plan is null)
            {
                if (await _workoutPlanRepository.ExistsAsync(request.Id, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout plan.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.WorkoutPlan), request.Id);
            }

            return _mapper.Map<WorkoutPlanDto>(plan);
        }
    }
}
