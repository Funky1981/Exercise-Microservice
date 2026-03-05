using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using Exercise.Application.Features.Auth.Dtos;
using MediatR;

namespace Exercise.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null || !user.VerifyPassword(request.Password))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return new LoginResponse
            {
                Token     = _tokenService.GenerateToken(user),
                ExpiresAt = _tokenService.GetExpiry(),
                UserId    = user.Id,
                Name      = user.Name,
                Email     = user.Email
            };
        }
    }
}
