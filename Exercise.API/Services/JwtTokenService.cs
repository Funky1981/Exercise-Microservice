using Exercise.Application.Abstractions.Services;
using Exercise.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Exercise.API.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name,  user.Name),
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             _configuration["Jwt:Issuer"],
                audience:           _configuration["Jwt:Audience"],
                claims:             claims,
                expires:            GetExpiry(),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetExpiry()
        {
            var days = int.TryParse(_configuration["Jwt:ExpiryDays"], out var d) ? d : 7;
            return DateTime.UtcNow.AddDays(days);
        }
    }
}
