using System.Data;
using EmotionService.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmotionService.Infrastructure.BackgroundJobs;

public sealed class AggregateWeightBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<AggregateWeightJobOptions> options,
    IConfiguration configuration,
    ILogger<AggregateWeightBackgroundService> logger)
    : BackgroundService
{
    private const string LockResource =
        "EmotionService.AggregateWeightJob";

    private readonly AggregateWeightJobOptions _options = options.Value;
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "The DefaultConnection connection string is required.");

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation(
                "Aggregate weight job is disabled.");
            return;
        }

        TimeZoneInfo timeZone;

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                _options.TimeZoneId);
        }
        catch (Exception exception)
            when (exception is TimeZoneNotFoundException
                or InvalidTimeZoneException)
        {
            logger.LogError(
                exception,
                "Aggregate weight job timezone {TimeZoneId} was not found.",
                _options.TimeZoneId);
            return;
        }

        if (_options.QueueAllOnStartup)
        {
            await MarkAllPendingAsync(stoppingToken);
        }

        if (_options.RunOnStartup)
        {
            await RunSafelyAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var nextRun = GetNextRunUtc(
                utcNow,
                timeZone);

            var delay = nextRun - utcNow;

            if (delay < TimeSpan.Zero)
            {
                delay = TimeSpan.Zero;
            }

            logger.LogInformation(
                "Next aggregate weight job is scheduled for {NextRunUtc} UTC.",
                nextRun);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await RunSafelyAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task MarkAllPendingAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var queue = scope.ServiceProvider
            .GetRequiredService<AggregateWeightQueue>();

        await queue.MarkAllPendingAsync(cancellationToken);

        logger.LogInformation(
            "All MediaItems with Experience or aggregate data were queued for weight rebuilding.");
    }

    private async Task RunSafelyAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await RunAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Aggregate weight job failed unexpectedly.");
        }
    }

    private async Task RunAsync(
        CancellationToken cancellationToken)
    {
        await using var lockConnection = new SqlConnection(
            _connectionString);

        await lockConnection.OpenAsync(cancellationToken);

        if (!await TryAcquireLockAsync(
                lockConnection,
                cancellationToken))
        {
            logger.LogInformation(
                "Aggregate weight job skipped because another instance is running.");
            return;
        }

        var startedAt = DateTime.UtcNow;
        var processedCount = 0;
        var retainedCount = 0;
        var failureCount = 0;

        logger.LogInformation(
            "Aggregate weight job started at {StartedAt} UTC.",
            startedAt);

        try
        {
            var pendingUpdates = await GetPendingUpdatesAsync(
                cancellationToken);

            foreach (var batch in pendingUpdates.Chunk(
                         _options.BatchSize))
            {
                foreach (var pendingUpdate in batch)
                {
                    try
                    {
                        await using var scope =
                            scopeFactory.CreateAsyncScope();

                        var processor = scope.ServiceProvider
                            .GetRequiredService<AggregateWeightProcessor>();

                        var markerRemoved = await processor.ProcessAsync(
                            pendingUpdate,
                            cancellationToken);

                        processedCount++;

                        if (!markerRemoved)
                        {
                            retainedCount++;
                        }
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        failureCount++;

                        logger.LogError(
                            exception,
                            "Aggregate weight processing failed for MediaItem {MediaItemId}.",
                            pendingUpdate.MediaItemId);
                    }
                }
            }
        }
        finally
        {
            await ReleaseLockAsync(
                lockConnection,
                CancellationToken.None);
        }

        var completedAt = DateTime.UtcNow;

        logger.LogInformation(
            "Aggregate weight job completed at {CompletedAt} UTC in {DurationMs} ms. "
            + "Processed: {ProcessedCount}, retained: {RetainedCount}, failed: {FailureCount}.",
            completedAt,
            (completedAt - startedAt).TotalMilliseconds,
            processedCount,
            retainedCount,
            failureCount);
    }

    private async Task<IReadOnlyList<PendingWeightUpdateSnapshot>>
        GetPendingUpdatesAsync(CancellationToken cancellationToken)
    {
        var maximumItems = checked(
            _options.BatchSize * _options.MaxBatchesPerRun);

        await using var scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await dbContext.PendingMediaItemWeightUpdates
            .AsNoTracking()
            .OrderBy(x => x.RequestedAt)
            .Take(maximumItems)
            .Select(x => new PendingWeightUpdateSnapshot(
                x.MediaItemId,
                x.RequestedAt,
                x.Version))
            .ToListAsync(cancellationToken);
    }

    private DateTimeOffset GetNextRunUtc(
        DateTimeOffset utcNow,
        TimeZoneInfo timeZone)
    {
        var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        var scheduledLocalTime = new DateTime(
            localNow.Year,
            localNow.Month,
            localNow.Day,
            _options.Hour,
            _options.Minute,
            0,
            DateTimeKind.Unspecified);

        if (scheduledLocalTime <= localNow.DateTime)
        {
            scheduledLocalTime = scheduledLocalTime.AddDays(1);
        }

        while (timeZone.IsInvalidTime(scheduledLocalTime))
        {
            scheduledLocalTime = scheduledLocalTime.AddMinutes(1);
        }

        var scheduledUtcTime = TimeZoneInfo.ConvertTimeToUtc(
            scheduledLocalTime,
            timeZone);

        return new DateTimeOffset(scheduledUtcTime);
    }

    private static async Task<bool> TryAcquireLockAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock
                @Resource = @resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Session',
                @LockTimeout = 0;
            SELECT @result;
            """;
        command.Parameters.Add(
            new SqlParameter("@resource", SqlDbType.NVarChar, 255)
            {
                Value = LockResource
            });

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result) >= 0;
    }

    private static async Task ReleaseLockAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            EXEC sys.sp_releaseapplock
                @Resource = @resource,
                @LockOwner = 'Session';
            """;
        command.Parameters.Add(
            new SqlParameter("@resource", SqlDbType.NVarChar, 255)
            {
                Value = LockResource
            });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
