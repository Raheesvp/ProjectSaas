using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Services;

// JwtService — generates JWT access tokens with tenant + role claims
// This is the core of the SSO pattern the senior built at AUB
//
// Token contains:
// → sub (userId)
// → email
// → tenant_id ← Gateway extracts this and forwards as header
// → role ← Used for RBAC authorization
// → jti (unique token ID — for token revocation tracking)
public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
        => _configuration = configuration;

    public string GenerateAccessToken(User user, Tenant tenant)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims embedded in JWT — readable by gateway and all services
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim("tenant_id", tenant.Id.ToString()),
            new Claim("tenant_name", tenant.Name),
            new Claim("tenant_subdomain", tenant.Subdomain),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("full_name", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: GetAccessTokenExpiry(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Cryptographically secure random token — not JWT
    // Stored in DB, opaque to client
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public DateTime GetAccessTokenExpiry()
    {
        var minutes = int.Parse(
            _configuration["JwtSettings:ExpiryMinutes"] ?? "60");
        return DateTime.UtcNow.AddMinutes(minutes);
    }

    public DateTime GetRefreshTokenExpiry()
        => DateTime.UtcNow.AddDays(7);
}