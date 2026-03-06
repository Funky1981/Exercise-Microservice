using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using Exercise.Application.Features.Auth.Dtos;
using MediatR;

namespace Exercise.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(IUserRepository userRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenService   = tokenService;
            _unitOfWork     = unitOfWork;
        }

        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null || !user.VerifyRefreshToken(request.RefreshToken))
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var newRefreshToken  = _tokenService.GenerateRefreshToken();
            var newRefreshExpiry = _tokenService.GetRefreshTokenExpiry();

            user.SetRefreshToken(newRefreshToken, newRefreshExpiry);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse
            {
                Token              = _tokenService.GenerateToken(user),
                ExpiresAt          = _tokenService.GetExpiry(),
                UserId             = user.Id,
                Name               = user.Name,
                Email              = user.Email,
                RefreshToken       = newRefreshToken,
                RefreshTokenExpiry = newRefreshExpiry
            };
        }
    }
}
