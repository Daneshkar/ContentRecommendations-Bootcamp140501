namespace EmotionService.Infrastructure.BackgroundJobs;

public sealed class AggregateWeightJobOptions
{
    public const string SectionName = "AggregateWeightJob";

    public bool Enabled { get; init; } = true;

    public int Hour { get; init; } = 5;

    public int Minute { get; init; }

    public string TimeZoneId { get; init; } = "Asia/Tehran";

    public int BatchSize { get; init; } = 100;

    public int MaxBatchesPerRun { get; init; } = 100;

    public bool RunOnStartup { get; init; }

    public bool QueueAllOnStartup { get; init; }
}
