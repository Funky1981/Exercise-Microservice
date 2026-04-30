using AutoMapper;
using Exercise.Application.Exercises.Dtos;
using Exercise.Domain.Entities;
using System.Text.Json;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Features.Exercises.Mapping
{
    public class ExerciseProfile : Profile
    {
        public ExerciseProfile()
        {
            CreateMap<ExerciseEntity, ExerciseDto>()
                .ForMember(dest => dest.SecondaryMuscles,
                    opt => opt.MapFrom(src => DeserializeJsonArray(src.SecondaryMusclesJson)))
                .ForMember(dest => dest.Instructions,
                    opt => opt.MapFrom(src => DeserializeJsonArray(src.InstructionsJson)));

            CreateMap<ExerciseMediaCandidate, ExerciseMediaCandidateDto>();
        }

        private static IReadOnlyList<string> DeserializeJsonArray(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}
