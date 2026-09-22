namespace EmotionService.Domain.Entities;

public class MediaItem
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; } = default!;

    public int ItemTypeId { get; private set; }

    public ItemType ItemType { get; private set; } = default!;

    public string? ImageUrl { get; private set; }

    public bool Status { get; private set; }

    public DateOnly? ReleaseDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private MediaItem() { }

    public static MediaItem Create(
        string name,
        string description,
        int itemTypeId,
        DateOnly? releaseDate,
        string? imageUrl = null
        )
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            ItemTypeId = itemTypeId,
            ImageUrl = imageUrl?.Trim(),
            ReleaseDate = releaseDate,
            Status = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(
        int itemTypeId,
        string name,
        string description,
        DateOnly? releaseDate,
        string? coverUrl)
    {
        ItemTypeId = itemTypeId;
        Name = name.Trim();
        Description = description.Trim();
        ReleaseDate = releaseDate;
        ImageUrl = coverUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = true;
        UpdatedAt = DateTime.UtcNow;
    }
}