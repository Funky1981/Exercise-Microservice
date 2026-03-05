using Exercise.Application.Features.Auth.Dtos;
using MediatR;

namespace Exercise.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
