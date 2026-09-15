using EmotionService.Application.Features.Experiences.Create;
using EmotionService.Application.Features.Experiences.Delete;
using EmotionService.Application.Features.Experiences.GetById;
using EmotionService.Application.Features.Experiences.GetMine;
using EmotionService.Application.Features.Experiences.Update;
using EmotionService.Contracts.Experiences;
using EmotionService.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Authorize]
[Route("api/experiences")]
public sealed class ExperiencesController(
    ISender sender,
    ICurrentUserService currentUserService)
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        var response = await sender.Send(
            new GetMyExperiencesQuery(userId),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        var response = await sender.Send(
            new GetExperienceByIdQuery(id, userId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        var response = await sender.Send(
            new UpdateExperienceCommand(
                id,
                userId,
                request.Score,
                request.Note,
                request.MoodIds ?? [],
                request.ThemeIds ?? []),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();

        await sender.Send(
            new DeleteExperienceCommand(id, userId),
            cancellationToken);

        return NoContent();
    }
}
