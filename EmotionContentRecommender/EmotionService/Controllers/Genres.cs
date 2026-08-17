using EmotionService.Application.Features.Genres.Create;
using EmotionService.Application.Features.Genres.GetAll;
using EmotionService.Application.Features.Genres.Update;
using EmotionService.Application.Features.Genres.GetById;
using EmotionService.Contracts.Genres;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Route("api/genres")]
public class GenreController : ControllerBase
{
    private readonly ISender _sender;

    public GenreController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGenreRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGenreCommand(
            request.ItemTypeId,
            request.Name,
            request.Description);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] int? itemTypeId,
    CancellationToken cancellationToken)
    {
        var query = new GetGenresQuery(itemTypeId);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
    int id,
    CancellationToken cancellationToken)
    {
        var query = new GetGenreByIdQuery(id);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateGenreRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateGenreCommand(
            id,
            request.ItemTypeId,
            request.Name,
            request.Description);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }
}