using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Commands.RefreshToken;
using IdentityService.Application.Commands.RegisterTenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

// AuthController — thin controller, delegates everything to MediatR
// Controllers only:
// 1. Parse HTTP request
// 2. Send command/query via MediatR
// 3. Map Result to HTTP response
// Zero business logic lives here
[ApiController]
[Route("api/identity")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Register a new tenant and admin user</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTenantCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Description });

        return CreatedAtAction(nameof(Register), result.Value);
    }

    /// <summary>Login and receive JWT + refresh token</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error.Description });

        return Ok(result.Value);
    }

    /// <summary>Refresh access token using refresh token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error.Description });

        return Ok(result.Value);
    }

    /// <summary>Get current user info — requires valid JWT</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(
            System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(
            System.Security.Claims.ClaimTypes.Role)?.Value;
        var tenantId = User.FindFirst("tenant_id")?.Value;

        return Ok(new { userId, email, role, tenantId });
    }
}