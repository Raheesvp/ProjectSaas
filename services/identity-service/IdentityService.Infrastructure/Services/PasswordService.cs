using IdentityService.Application.Interfaces;

namespace IdentityService.Infrastructure.Services;

// BCrypt implementation — industry standard for password hashing
// Work factor 12 — computationally expensive enough to resist brute force
// Real world: Same work factor used in GitHub, Shopify password systems
public sealed class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}