using EmotionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Infrastructure.BackgroundJobs;

public sealed class AggregateWeightQueue(
    ApplicationDbContext dbContext)
{
    public Task MarkPendingAsync(
        Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        var requestedAt = DateTime.UtcNow;

        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            MERGE INTO [PendingMediaItemWeightUpdates] WITH (HOLDLOCK) AS target
            USING (VALUES ({mediaItemId}, {requestedAt}))
                AS source ([MediaItemId], [RequestedAt])
            ON target.[MediaItemId] = source.[MediaItemId]
            WHEN MATCHED THEN
                UPDATE SET
                    [RequestedAt] = source.[RequestedAt],
                    [Version] = target.[Version] + 1
            WHEN NOT MATCHED THEN
                INSERT ([MediaItemId], [RequestedAt], [Version])
                VALUES (source.[MediaItemId], source.[RequestedAt], 1);
            """,
            cancellationToken);
    }

    public Task MarkAllPendingAsync(
        CancellationToken cancellationToken)
    {
        var requestedAt = DateTime.UtcNow;

        return dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            MERGE INTO [PendingMediaItemWeightUpdates] WITH (HOLDLOCK) AS target
            USING
            (
                SELECT candidates.[MediaItemId], {requestedAt} AS [RequestedAt]
                FROM
                (
                    SELECT [MediaItemId] FROM [Experiences]
                    UNION
                    SELECT [MediaItemId] FROM [ItemMoodWeights]
                    UNION
                    SELECT [MediaItemId] FROM [ItemThemeWeights]
                ) AS candidates
            ) AS source
            ON target.[MediaItemId] = source.[MediaItemId]
            WHEN MATCHED THEN
                UPDATE SET
                    [RequestedAt] = source.[RequestedAt],
                    [Version] = target.[Version] + 1
            WHEN NOT MATCHED THEN
                INSERT ([MediaItemId], [RequestedAt], [Version])
                VALUES (source.[MediaItemId], source.[RequestedAt], 1);
            """,
            cancellationToken);
    }
}
