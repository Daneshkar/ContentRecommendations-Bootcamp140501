namespace EmotionService.Domain.Entities;

public class Genre
{
    public int Id { get; private set; }

    public int ItemTypeId { get; private set; }

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    private Genre()
    {
    }

    public static Genre Create(
        int itemTypeId,
        string name,
        string? description = null)
    {
        if (itemTypeId <= 0)
            throw new ArgumentException("Item type is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Genre name is required.");

        return new Genre
        {
            ItemTypeId = itemTypeId,
            Name = name.Trim(),
            Description = description?.Trim()
        };
    }

    public void Update(
        int itemTypeId,
        string name,
        string? description)
    {
        if (itemTypeId <= 0)
            throw new ArgumentException("Item type is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Genre name is required.");

        ItemTypeId = itemTypeId;
        Name = name.Trim();
        Description = description?.Trim();
    }
}