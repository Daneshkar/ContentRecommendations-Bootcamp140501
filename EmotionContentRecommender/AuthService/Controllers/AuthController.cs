using AuthService.Application.Common;
using AuthService.Application.Features.Auth.Login;
using AuthService.Application.Features.Auth.Logout;
using AuthService.Application.Features.Auth.RefreshToken;
using AuthService.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResult<LoginResponse>),    200)]
    [ProducesResponseType(typeof(ApiResult<LoginResponse>),    401)]
    [ProducesResponseType(typeof(ApiResult<LoginResponse>),    422)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResult<RegisterResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<RegisterResponse>), 409)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResult<RefreshTokenResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<RefreshTokenResponse>), 401)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        // Refresh Token از Cookie خوانده می‌شود
        var refreshToken = Request.Cookies["refresh_token"];

        var result = await _mediator.Send(
            new RefreshTokenCommand(refreshToken ?? string.Empty), ct);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResult), 200)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var result = await _mediator.Send(new LogoutCommand(), ct);
        return StatusCode(result.StatusCode, result);
    }
}
