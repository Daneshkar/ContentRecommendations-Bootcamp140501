using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Experiences.Create;

public sealed class CreateExperienceCommandHandler(
    ApplicationDbContext dbContext)
    : IRequestHandler<CreateExperienceCommand, CreateExperienceResponse>
{
    public async Task<CreateExperienceResponse> Handle(
        CreateExperienceCommand command,
        CancellationToken cancellationToken)
    {
        var mediaItemExists = await dbContext.MediaItems
            .AnyAsync(
                x => x.Id == command.MediaItemId && x.Status,
                cancellationToken);

        if (!mediaItemExists)
        {
            throw new NotFoundException(
                "محتوای انتخاب‌شده وجود ندارد یا غیرفعال است.");
        }

        var experienceAlreadyExists = await dbContext.Experiences
            .AnyAsync(
                x => x.UserId == command.UserId
                    && x.MediaItemId == command.MediaItemId,
                cancellationToken);

        if (experienceAlreadyExists)
        {
            throw new ConflictException(
                "تجربه‌ی این محتوا قبلاً توسط کاربر ثبت شده است.");
        }

        await EnsureMoodsAreActive(command.MoodIds, cancellationToken);
        await EnsureThemesAreActive(command.ThemeIds, cancellationToken);

        var userWeight = ExperienceUserWeightCalculator.FromScore(
            command.Score);

        var experience = Experience.Create(
            command.UserId,
            command.Score,
            command.MediaItemId,
            command.Note);

        var experienceMoods = command.MoodIds
            .Select(moodId => ExperienceMood.Create(
                experience.Id,
                moodId,
                userWeight))
            .ToArray();

        var experienceThemes = command.ThemeIds
            .Select(themeId => ExperienceTheme.Create(
                experience.Id,
                themeId,
                userWeight))
            .ToArray();

        dbContext.Experiences.Add(experience);
        dbContext.ExperienceMoods.AddRange(experienceMoods);
        dbContext.ExperienceThemes.AddRange(experienceThemes);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateExperienceResponse(
            experience.Id,
            experience.UserId,
            experience.MediaItemId,
            experience.Score,
            experience.Note,
            command.MoodIds,
            command.ThemeIds,
            experience.CreatedAt);
    }

    private async Task EnsureMoodsAreActive(
        IReadOnlyCollection<int> moodIds,
        CancellationToken cancellationToken)
    {
        var activeMoodCount = await dbContext.Moods
            .CountAsync(
                x => moodIds.Contains(x.Id) && x.IsActive,
                cancellationToken);

        if (activeMoodCount != moodIds.Count)
        {
            throw new BadRequestException(
                "یک یا چند حالت احساسی وجود ندارند یا غیرفعال هستند.");
        }
    }

    private async Task EnsureThemesAreActive(
        IReadOnlyCollection<int> themeIds,
        CancellationToken cancellationToken)
    {
        var activeThemeCount = await dbContext.Themes
            .CountAsync(
                x => themeIds.Contains(x.Id) && x.IsActive,
                cancellationToken);

        if (activeThemeCount != themeIds.Count)
        {
            throw new BadRequestException(
                "یک یا چند تم وجود ندارند یا غیرفعال هستند.");
        }
    }
}
