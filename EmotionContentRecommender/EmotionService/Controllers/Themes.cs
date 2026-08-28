using EmotionService.Application.Features.Themes.Create;
using EmotionService.Application.Features.Themes.GetById;
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
}