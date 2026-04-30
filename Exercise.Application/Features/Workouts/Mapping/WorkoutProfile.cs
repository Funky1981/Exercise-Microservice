using AutoMapper;
using Exercise.Application.Features.Workouts.Dtos;
using Exercise.Domain.Entities;

namespace Exercise.Application.Features.Workouts.Mapping
{
    public class WorkoutProfile : Profile
    {
        public WorkoutProfile()
        {
            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.BodyPart, opt => opt.MapFrom(src => src.Exercise.BodyPart))
                .ForMember(dest => dest.TargetMuscle, opt => opt.MapFrom(src => src.Exercise.TargetMuscle))
                .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Exercise.Equipment))
                .ForMember(dest => dest.GifUrl, opt => opt.MapFrom(src => src.Exercise.GifUrl))
                .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => src.Exercise.MediaUrl))
                .ForMember(dest => dest.MediaKind, opt => opt.MapFrom(src => src.Exercise.MediaKind))
                .ForMember(dest => dest.MediaThumbnailUrl, opt => opt.MapFrom(src => src.Exercise.MediaThumbnailUrl))
                .ForMember(dest => dest.MediaSourceProvider, opt => opt.MapFrom(src => src.Exercise.MediaSourceProvider));

            CreateMap<Workout, WorkoutDto>()
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.WorkoutExercises));
        }
    }
}
