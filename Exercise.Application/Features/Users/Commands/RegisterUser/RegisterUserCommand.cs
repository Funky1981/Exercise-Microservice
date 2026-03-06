using MediatR;

namespace Exercise.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? UserName { get; set; }
    }
}
