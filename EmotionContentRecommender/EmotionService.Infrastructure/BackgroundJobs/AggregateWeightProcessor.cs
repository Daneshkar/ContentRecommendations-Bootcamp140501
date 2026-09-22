using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Infrastructure.BackgroundJobs;

public sealed record PendingWeightUpdateSnapshot(
    Guid MediaItemId,
    DateTime RequestedAt,
    long Version);

public sealed class AggregateWeightProcessor(
    ApplicationDbContext dbContext)
{
    private const int PriorExperienceCount = 10;
    private const decimal InitialWeight = 0.05m;

    public async Task<bool> ProcessAsync(
        PendingWeightUpdateSnapshot pendingUpdate,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        await SynchronizeMoodWeightsAsync(
            pendingUpdate.MediaItemId,
            cancellationToken);

        await SynchronizeThemeWeightsAsync(
            pendingUpdate.MediaItemId,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var removedPendingMarker = await dbContext
            .PendingMediaItemWeightUpdates
            .Where(x =>
                x.MediaItemId == pendingUpdate.MediaItemId
                && x.Version == pendingUpdate.Version)
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return removedPendingMarker == 1;
    }

    private async Task SynchronizeMoodWeightsAsync(
        Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        var statistics = await dbContext.ExperienceMoods
            .AsNoTracking()
            .Where(x => x.Experience.MediaItemId == mediaItemId)
            .GroupBy(x => x.MoodId)
            .Select(group => new
            {
                MoodId = group.Key,
                ExperienceCount = group.Count(),
                UserWeightSum = group.Sum(x => x.UserWeight)
            })
            .ToListAsync(cancellationToken);

        var existingWeights = await dbContext.ItemMoodWeights
            .Where(x => x.MediaItemId == mediaItemId)
            .ToDictionaryAsync(x => x.MoodId, cancellationToken);

        foreach (var statistic in statistics)
        {
            var weightValue = CalculateWeight(
                statistic.UserWeightSum,
                statistic.ExperienceCount);

            if (existingWeights.TryGetValue(
                    statistic.MoodId,
                    out var existingWeight))
            {
                existingWeight.UpdateWeight(
                    weightValue,
                    statistic.ExperienceCount);
            }
            else
            {
                dbContext.ItemMoodWeights.Add(
                    ItemMoodWeight.Create(
                        mediaItemId,
                        statistic.MoodId,
                        weightValue,
                        statistic.ExperienceCount));
            }
        }

        var currentMoodIds = statistics
            .Select(x => x.MoodId)
            .ToHashSet();

        dbContext.ItemMoodWeights.RemoveRange(
            existingWeights.Values.Where(
                x => !currentMoodIds.Contains(x.MoodId)));
    }

    private async Task SynchronizeThemeWeightsAsync(
        Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        var statistics = await dbContext.ExperienceThemes
            .AsNoTracking()
            .Where(x => x.Experience.MediaItemId == mediaItemId)
            .GroupBy(x => x.ThemeId)
            .Select(group => new
            {
                ThemeId = group.Key,
                ExperienceCount = group.Count(),
                UserWeightSum = group.Sum(x => x.UserWeight)
            })
            .ToListAsync(cancellationToken);

        var existingWeights = await dbContext.ItemThemeWeights
            .Where(x => x.MediaItemId == mediaItemId)
            .ToDictionaryAsync(x => x.ThemeId, cancellationToken);

        foreach (var statistic in statistics)
        {
            var weightValue = CalculateWeight(
                statistic.UserWeightSum,
                statistic.ExperienceCount);

            if (existingWeights.TryGetValue(
                    statistic.ThemeId,
                    out var existingWeight))
            {
                existingWeight.UpdateWeight(
                    weightValue,
                    statistic.ExperienceCount);
            }
            else
            {
                dbContext.ItemThemeWeights.Add(
                    ItemThemeWeight.Create(
                        mediaItemId,
                        statistic.ThemeId,
                        weightValue,
                        statistic.ExperienceCount));
            }
        }

        var currentThemeIds = statistics
            .Select(x => x.ThemeId)
            .ToHashSet();

        dbContext.ItemThemeWeights.RemoveRange(
            existingWeights.Values.Where(
                x => !currentThemeIds.Contains(x.ThemeId)));
    }

    private static decimal CalculateWeight(
        decimal userWeightSum,
        int experienceCount)
    {
        var weight =
            (PriorExperienceCount * InitialWeight + userWeightSum)
            / (PriorExperienceCount + experienceCount);

        return decimal.Round(
            weight,
            decimals: 5,
            MidpointRounding.AwayFromZero);
    }
}
