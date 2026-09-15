using EmotionService.Application.Features.Experiences.Common;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Experiences.GetById;

public sealed class GetExperienceByIdQueryHandler(
    ApplicationDbContext dbContext)
    : IRequestHandler<GetExperienceByIdQuery, ExperienceResponse>
{
    public async Task<ExperienceResponse> Handle(
        GetExperienceByIdQuery query,
        CancellationToken cancellationToken)
    {
        var experience = await dbContext.Experiences
            .AsNoTracking()
            .Where(x => x.Id == query.Id && x.UserId == query.UserId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (experience is null)
        {
            throw new NotFoundException("تجربه‌ی مورد نظر یافت نشد.");
        }

        return experience;
    }
}
