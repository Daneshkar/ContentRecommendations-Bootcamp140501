using System.Security.Claims;
using EmotionService.Application.Features.Experiences.Create;
using EmotionService.Contracts.Experiences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Authorize]
[Route("api/experiences")]
public sealed class ExperiencesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!long.TryParse(userIdClaim, out var userId) || userId <= 0)
        {
            return Unauthorized();
        }

        var command = new CreateExperienceCommand(
            userId,
            request.MediaItemId,
            request.Score,
            request.Note,
            request.MoodIds ?? [],
            request.ThemeIds ?? []);

        var response = await sender.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }
}
