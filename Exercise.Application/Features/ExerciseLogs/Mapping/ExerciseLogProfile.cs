using AutoMapper;
using Exercise.Application.Features.ExerciseLogs.Dtos;
using Exercise.Domain.Entities;

namespace Exercise.Application.Features.ExerciseLogs.Mapping
{
    public class ExerciseLogProfile : Profile
    {
        public ExerciseLogProfile()
        {
            CreateMap<ExerciseLogEntry, ExerciseLogEntryDto>();

            CreateMap<ExerciseLog, ExerciseLogDto>()
                .ForMember(dest => dest.Entries, opt => opt.MapFrom(src => src.ExercisesCompleted));
        }
    }
}
