
using IdentityService.Domain.Enums;
using Shared.Domain.Primitives;

namespace IdentityService.Domain.Entities;


   public sealed class User : AggregateRoot<Guid>

{
    private readonly List<RefreshToken> _refreshTokens = new(); //no one can hack the refresh token to hack.every time new token is added.

    private User()  {}

    private User(
        Guid id,
        Guid tenantId,
        string email,
        string fullName,
        string passwordHash,
        UserRole role) : base(id)
    {
        TenantId = tenantId;
        Email = email;
        FullName = fullName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }


    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; } 

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    //Safety first approach to user creation.Developer assigns a role ,new user gets lowest permissible .
    public static User Create(
        Guid tenantId,
        string email,
        string fullName,
        string passwordHash,
        UserRole role = UserRole.Viewer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(Guid.NewGuid(), tenantId, email, fullName, passwordHash, role);
    }

     // Called after successful login
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

    // Add new refresh token — called after JWT generation
    public void AddRefreshToken(RefreshToken token)
    {
        // Revoke all existing active tokens before adding new one
        // This enforces single-session per user (can be relaxed for multi-device)
        RevokeAllRefreshTokens("Replaced by new token");
        _refreshTokens.Add(token);
    }

    //check if token is provided by user is actually valid.If user clicks the logout button , we will kill the token .

    public RefreshToken? GetActiveRefreshToken(string token)
        => _refreshTokens.FirstOrDefault(t =>
            t.Token == token &&
            !t.IsRevoked &&
            t.ExpiresAt > DateTime.UtcNow);

    public void RevokeRefreshToken(string token, string reason)
    {
        var refreshToken = GetActiveRefreshToken(token);
        refreshToken?.Revoke(reason);
    }

    private void RevokeAllRefreshTokens(string reason)
    {
        foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
            token.Revoke(reason);
    }

    public void Deactivate() => IsActive = false;
    public void ChangeRole(UserRole newRole) => Role = newRole;

    
}
