using AutoMapper;
using Exercise.Application.Features.Users.Dtos;
using Exercise.Domain.Entities;

namespace Exercise.Application.Features.Users.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.HeightCm, o => o.MapFrom(s => s.Height != null ? (decimal?)s.Height.Centimeters : null))
                .ForMember(d => d.WeightKg, o => o.MapFrom(s => s.Weight != null ? (decimal?)s.Weight.Kilograms : null));
        }
    }
}
