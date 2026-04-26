using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TradingAlpha.Application.Interfaces;
using TradingAlpha.Domain.Entities;

namespace TradingAlpha.Infrastructure.Authentication;

/// <summary>
/// JwtTokenGenerator — Creates signed JWT tokens for authenticated users.
/// 
/// Implements IJwtTokenGenerator (defined in Application layer).
/// Uses HMAC-SHA256 symmetric signing with the key from JwtSettings.
/// 
/// Claims included in the token:
///   - sub (subject):   User's GUID — used to identify the user on subsequent requests
///   - username:        For display purposes
///   - name:            For display purposes
///   - role:            For role-based authorization ([Authorize(Roles = "Admin")])
///   - jti:             Unique token ID — useful for token revocation later
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        // Build the signing key from the configured secret
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Define claims — these are embedded in the JWT payload
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("username", user.Username),
            new Claim("name", user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        // Create the token
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiresAt);
    }
}