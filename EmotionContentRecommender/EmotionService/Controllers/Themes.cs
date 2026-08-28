using EmotionService.Application.Features.Themes.Create;
using EmotionService.Application.Features.Themes.GetById;
using EmotionService.Application.Features.Themes.GetAll;
using EmotionService.Application.Features.Themes.Update;
using EmotionService.Application.Features.Themes.ChangeStatus;
using EmotionService.Contracts.Themes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Route("api/themes")]
public sealed class ThemeController : ControllerBase
{
    private readonly ISender _sender;

    public ThemeController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateThemeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateThemeCommand(
            request.Name,
            request.Description);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
    int id,
    CancellationToken cancellationToken)
    {
        var query = new GetThemeByIdQuery(id);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] bool? isActive,
    CancellationToken cancellationToken)
    {
        var query = new GetThemesQuery(isActive);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateThemeRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateThemeCommand(
            id,
            request.Name,
            request.Description);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(
    int id,
    CancellationToken cancellationToken)
    {
        var command = new ChangeThemeStatusCommand(
            id,
            true);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        var command = new ChangeThemeStatusCommand(
            id,
            false);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }
}