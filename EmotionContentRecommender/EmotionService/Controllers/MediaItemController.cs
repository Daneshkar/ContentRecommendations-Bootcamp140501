using EmotionService.Application.Features.MediaItems.Create;
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
}