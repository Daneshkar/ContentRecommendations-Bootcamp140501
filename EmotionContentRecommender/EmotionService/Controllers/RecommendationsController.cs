using EmotionService.Application.Features.Recommendations.Get;
using EmotionService.Application.Features.Recommendations.GetByExperience;
using EmotionService.Contracts.Recommendations;
using EmotionService.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmotionService.Controllers;

[ApiController]
[Authorize]
[Route("api/recommendations")]
public sealed class RecommendationsController(
    ISender sender,
    ICurrentUserService currentUserService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Get(
        [FromBody] GetRecommendationsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new GetRecommendationsQuery(
                currentUserService.GetUserId(),
                request.ItemTypeId,
                request.PrimaryMoodId,
                request.AdditionalMoodIds ?? [],
                request.ThemeIds ?? [],
                request.PageSize ?? 10,
                request.Cursor),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("by-experience")]
    public async Task<IActionResult> GetByExperience(
        [FromBody] GetExperienceRecommendationsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new GetExperienceRecommendationsQuery(
                currentUserService.GetUserId(),
                request.ExperienceId,
                request.PageSize ?? 10,
                request.Cursor),
            cancellationToken);

        return Ok(response);
    }
}
