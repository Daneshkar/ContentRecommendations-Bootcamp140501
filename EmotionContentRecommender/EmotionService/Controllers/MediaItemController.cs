using EmotionService.Application.Features.MediaItems.Create;
using EmotionService.Application.Features.MediaItems.GetById;
using EmotionService.Application.Features.MediaItems.Deactivate;
using EmotionService.Application.Features.MediaItems.Update;
using EmotionService.Application.Features.MediaItems.GetAll;
using EmotionService.Application.Features.MediaItemGenres.Assign;
using EmotionService.Contracts.MediaItems;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Route("api/media-items")]
public class MediaItemController : ControllerBase
{
    private readonly ISender _sender;

    public MediaItemController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMediaItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMediaItemCommand(
            request.ItemTypeId,
            request.Name,
            request.Description,
            request.ReleaseDate,
            request.CoverUrl
        );

        var response = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var query = new GetMediaItemByIdQuery(id);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] int? itemTypeId,
    [FromQuery] string? search,
    CancellationToken cancellationToken)
    {
        var query = new GetMediaItemsQuery(
            itemTypeId,
            search);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
    Guid id,
    [FromBody] UpdateMediaItemRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateMediaItemCommand(
            id,
            request.ItemTypeId,
            request.Name,
            request.Description,
            request.ReleaseDate,
            request.CoverUrl
        );

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(
    Guid id,
    CancellationToken cancellationToken)
    {
        var command = new DeactivateMediaItemCommand(id);

        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{mediaItemId:guid}/genres/{genreId:int}")]
    public async Task<IActionResult> AssignGenre(
    Guid mediaItemId,
    int genreId,
    CancellationToken cancellationToken)
    {
        await _sender.Send(
            new AssignGenreToMediaItemCommand(
                mediaItemId,
                genreId),
            cancellationToken);

        return NoContent();
    }
}