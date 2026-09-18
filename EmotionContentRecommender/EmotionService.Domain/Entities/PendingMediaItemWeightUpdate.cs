namespace EmotionService.Domain.Entities;

public sealed class PendingMediaItemWeightUpdate
{
    public Guid MediaItemId { get; private set; }

    public MediaItem MediaItem { get; private set; } = default!;

    public DateTime RequestedAt { get; private set; }

    public long Version { get; private set; }

    private PendingMediaItemWeightUpdate()
    {
    }
}
