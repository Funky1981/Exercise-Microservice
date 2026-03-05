using Exercise.Domain.Entities;

namespace Exercise.Application.Abstractions.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        DateTime GetExpiry();
    }
}
