using EmotionService.Application.Features.Experiences.Common;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Experiences.GetMine;

public sealed class GetMyExperiencesQueryHandler(
    ApplicationDbContext dbContext)
    : IRequestHandler<GetMyExperiencesQuery, IReadOnlyList<ExperienceResponse>>
{
    public async Task<IReadOnlyList<ExperienceResponse>> Handle(
        GetMyExperiencesQuery query,
        CancellationToken cancellationToken)
    {
        return await dbContext.Experiences
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ExperienceResponse(
                x.Id,
                x.MediaItemId,
                x.MediaItem.Name,
                x.MediaItem.ImageUrl,
                x.Score,
                x.Note,
                x.ExperienceMoods
                    .OrderBy(item => item.Mood.Name)
                    .Select(item => new ExperienceMoodResponse(
                        item.MoodId,
                        item.Mood.Name))
                    .ToArray(),
                x.ExperienceThemes
                    .OrderBy(item => item.Theme.Name)
                    .Select(item => new ExperienceThemeResponse(
                        item.ThemeId,
                        item.Theme.Name))
                    .ToArray(),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
