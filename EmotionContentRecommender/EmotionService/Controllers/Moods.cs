using EmotionService.Application.Features.Moods.Create;
using EmotionService.Application.Features.Moods.GetById;
using EmotionService.Contracts.Moods;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Route("api/moods")]
public sealed class MoodController : ControllerBase
{
    private readonly ISender _sender;

    public MoodController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMoodRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMoodCommand(
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
        var query = new GetMoodByIdQuery(id);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }
}