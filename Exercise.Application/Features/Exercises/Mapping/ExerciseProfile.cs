using AutoMapper;
using Exercise.Application.Exercises.Dtos;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Features.Exercises.Mapping
{
    public class ExerciseProfile : Profile
    {
        public ExerciseProfile()
        {
            CreateMap<ExerciseEntity, ExerciseDto>()
                .ReverseMap();
        }
    }
}
