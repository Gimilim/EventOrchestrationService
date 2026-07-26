using System.Security.Claims;
using System.Text;
using EventOrchestrationService.Application.Interfaces;
using EventOrchestrationService.Application.Settings;
using EventOrchestrationService.Domain.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EventOrchestrationService.Infrastructure.Security;

public class JwtService(JwtSettings jwtSettings) : IJwtService
{
    public string GenerateToken(int id, string login, Role role)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = id,
            [JwtRegisteredClaimNames.PreferredUsername] = login,
            [ClaimTypes.Role] = role.ToString(),
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            Claims = claims,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = creds
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}