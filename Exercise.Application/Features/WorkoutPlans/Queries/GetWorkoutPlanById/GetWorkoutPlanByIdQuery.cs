using Exercise.Application.Features.WorkoutPlans.Dtos;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlanById
{
    public class GetWorkoutPlanByIdQuery : IRequest<WorkoutPlanDto?>
    {
        public Guid Id { get; set; }
    }
}
