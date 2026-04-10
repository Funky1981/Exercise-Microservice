using AutoMapper;
using Exercise.Application.Features.WorkoutPlans.Dtos;
using Exercise.Domain.Entities;
using System.Linq;

namespace Exercise.Application.Features.WorkoutPlans.Mapping
{
    public class WorkoutPlanProfile : Profile
    {
        public WorkoutPlanProfile()
        {
            CreateMap<Workout, WorkoutPlanWorkoutDto>()
                .ForMember(dest => dest.ExerciseIds, opt => opt.MapFrom(src => src.WorkoutExercises.Select(we => we.ExerciseId)));
            CreateMap<WorkoutPlan, WorkoutPlanDto>();
        }
    }
}
