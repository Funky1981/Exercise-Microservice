using AutoMapper;
using Exercise.Application.Features.Workouts.Dtos;
using Exercise.Domain.Entities;

namespace Exercise.Application.Features.Workouts.Mapping
{
    public class WorkoutProfile : Profile
    {
        public WorkoutProfile()
        {
            CreateMap<Workout, WorkoutDto>();
        }
    }
}
