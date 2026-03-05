using MediatR;

namespace Exercise.Application.Features.Users.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
    }
}
