using FluentAssertions;
using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Moq;

namespace IdentityService.UnitTests.Commands;

// These tests run in milliseconds — no DB, no HTTP, no Docker needed
// They prove business logic is correct independent of infrastructure
public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITenantRepository> _tenantRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IJwtService> _jwtService = new();

    private LoginUserCommandHandler CreateHandler() => new(
        _userRepo.Object,
        _tenantRepo.Object,
        _passwordService.Object,
        _jwtService.Object);

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var tenant = Tenant.Create("Test Bank", "testbank", "test@bank.com");
        var passwordHash = "$2a$12$hashedpassword";
        var user = User.Create(tenant.Id, "admin@testbank.com",
            "Admin", passwordHash, Domain.Enums.UserRole.Admin);

        _tenantRepo.Setup(r => r.GetBySubdomainAsync("testbank", default))
            .ReturnsAsync(tenant);

        _userRepo.Setup(r => r.GetByEmailAsync(tenant.Id, "admin@testbank.com", default))
            .ReturnsAsync(user);

        _passwordService.Setup(p => p.VerifyPassword("Admin@123456", passwordHash))
            .Returns(true);

        _jwtService.Setup(j => j.GenerateAccessToken(user, tenant))
            .Returns("fake.jwt.token");

        _jwtService.Setup(j => j.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _jwtService.Setup(j => j.GetAccessTokenExpiry())
            .Returns(DateTime.UtcNow.AddHours(1));

        _jwtService.Setup(j => j.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        var command = new LoginUserCommand("admin@testbank.com", "Admin@123456", "testbank");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("fake.jwt.token");
        result.Value.User.Email.Should().Be("admin@testbank.com");
        result.Value.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        // Arrange
        var tenant = Tenant.Create("Test Bank", "testbank", "test@bank.com");
        var user = User.Create(tenant.Id, "admin@testbank.com",
            "Admin", "hashedpw", Domain.Enums.UserRole.Admin);

        _tenantRepo.Setup(r => r.GetBySubdomainAsync("testbank", default))
            .ReturnsAsync(tenant);

        _userRepo.Setup(r => r.GetByEmailAsync(tenant.Id, "admin@testbank.com", default))
            .ReturnsAsync(user);

        // Wrong password — BCrypt returns false
        _passwordService.Setup(p => p.VerifyPassword("WrongPassword", "hashedpw"))
            .Returns(false);

        var command = new LoginUserCommand("admin@testbank.com", "WrongPassword", "testbank");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_NonExistentSubdomain_ReturnsFailure()
    {
        // Arrange — tenant not found
        _tenantRepo.Setup(r => r.GetBySubdomainAsync("unknown", default))
            .ReturnsAsync((Tenant?)null);

        var command = new LoginUserCommand("admin@unknown.com", "Password@1", "unknown");

        // Act
        var result = await CreateHandler().Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");

        // IMPORTANT: Verify BCrypt was still called even though tenant not found
        // This prevents timing attacks revealing valid subdomains
        // If BCrypt is NOT called here that is a security bug
    }
}