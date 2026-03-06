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

            if (user is null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            // ── Token-reuse / replay attack detection ────────────────────────────
            // If the presented token matches the PREVIOUS (already-rotated) hash,
            // an attacker is replaying a stolen old token.  Revoke everything and
            // let the legitimate user re-authenticate from scratch.
            if (user.IsRefreshTokenReused(request.RefreshToken))
            {
                user.ClearRefreshToken();
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException("Token reuse detected — session revoked. Please log in again.");
            }

            if (!user.VerifyRefreshToken(request.RefreshToken))
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
