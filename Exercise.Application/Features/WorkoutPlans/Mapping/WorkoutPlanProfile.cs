using AutoMapper;
using Exercise.Application.Features.WorkoutPlans.Dtos;
using Exercise.Domain.Entities;

namespace Exercise.Application.Features.WorkoutPlans.Mapping
{
    public class WorkoutPlanProfile : Profile
    {
        public WorkoutPlanProfile()
        {
            CreateMap<WorkoutPlan, WorkoutPlanDto>();
        }
    }
}
