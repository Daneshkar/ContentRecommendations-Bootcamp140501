using AuthService.Application.Common;
using AuthService.Application.Features.Auth.GetProfile;
using AuthService.Application.Features.Auth.Login;
using AuthService.Application.Features.Auth.Logout;
using AuthService.Application.Features.Auth.RefreshToken;
using AuthService.Application.Features.Auth.Register;
using AuthService.Application.Features.Otp.SendOtp;
using AuthService.Application.Features.Otp.VerifyOtp;
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

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResult<ProfileResponse>), 200)]
    [ProducesResponseType(typeof(ApiResult<ProfileResponse>), 401)]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProfileQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("send-otp")]
    [ProducesResponseType(typeof(ApiResult), 200)]
    [ProducesResponseType(typeof(ApiResult), 404)]
    [ProducesResponseType(typeof(ApiResult), 502)]
    public async Task<IActionResult> SendOtp(
        [FromBody] SendOtpCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(ApiResult), 200)]
    [ProducesResponseType(typeof(ApiResult), 400)]
    [ProducesResponseType(typeof(ApiResult), 404)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }
}
