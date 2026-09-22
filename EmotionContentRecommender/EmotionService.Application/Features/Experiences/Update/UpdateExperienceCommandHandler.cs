using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.BackgroundJobs;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Experiences.Update;

public sealed class UpdateExperienceCommandHandler(
    ApplicationDbContext dbContext,
    AggregateWeightQueue aggregateWeightQueue)
    : IRequestHandler<UpdateExperienceCommand, UpdateExperienceResponse>
{
    public async Task<UpdateExperienceResponse> Handle(
        UpdateExperienceCommand command,
        CancellationToken cancellationToken)
    {
        var experience = await dbContext.Experiences
            .Include(x => x.ExperienceMoods)
            .Include(x => x.ExperienceThemes)
            .FirstOrDefaultAsync(
                x => x.Id == command.Id && x.UserId == command.UserId,
                cancellationToken);

        if (experience is null)
        {
            throw new NotFoundException("تجربه‌ی مورد نظر یافت نشد.");
        }

        await EnsureMoodsAreActive(command.MoodIds, cancellationToken);
        await EnsureThemesAreActive(command.ThemeIds, cancellationToken);

        var userWeight = ExperienceUserWeightCalculator.FromScore(
            command.Score);

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        experience.UpdateScore(command.Score);
        experience.UpdateNote(command.Note);

        SynchronizeMoods(experience, command.MoodIds, userWeight);
        SynchronizeThemes(experience, command.ThemeIds, userWeight);

        await dbContext.SaveChangesAsync(cancellationToken);

        await aggregateWeightQueue.MarkPendingAsync(
            experience.MediaItemId,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new UpdateExperienceResponse(
            experience.Id,
            experience.MediaItemId,
            experience.Score,
            experience.Note,
            command.MoodIds,
            command.ThemeIds,
            experience.UpdatedAt);
    }

    private void SynchronizeMoods(
        Experience experience,
        IReadOnlyCollection<int> moodIds,
        decimal userWeight)
    {
        var selectedIds = moodIds.ToHashSet();
        var existingMoods = experience.ExperienceMoods.ToArray();

        dbContext.ExperienceMoods.RemoveRange(
            existingMoods.Where(x => !selectedIds.Contains(x.MoodId)));

        foreach (var existingMood in existingMoods
                     .Where(x => selectedIds.Contains(x.MoodId)))
        {
            existingMood.UpdateWeight(userWeight);
        }

        var existingIds = existingMoods
            .Select(x => x.MoodId)
            .ToHashSet();

        dbContext.ExperienceMoods.AddRange(
            moodIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => ExperienceMood.Create(
                    experience.Id,
                    id,
                    userWeight)));
    }

    private void SynchronizeThemes(
        Experience experience,
        IReadOnlyCollection<int> themeIds,
        decimal userWeight)
    {
        var selectedIds = themeIds.ToHashSet();
        var existingThemes = experience.ExperienceThemes.ToArray();

        dbContext.ExperienceThemes.RemoveRange(
            existingThemes.Where(x => !selectedIds.Contains(x.ThemeId)));

        foreach (var existingTheme in existingThemes
                     .Where(x => selectedIds.Contains(x.ThemeId)))
        {
            existingTheme.UpdateWeight(userWeight);
        }

        var existingIds = existingThemes
            .Select(x => x.ThemeId)
            .ToHashSet();

        dbContext.ExperienceThemes.AddRange(
            themeIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => ExperienceTheme.Create(
                    experience.Id,
                    id,
                    userWeight)));
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
