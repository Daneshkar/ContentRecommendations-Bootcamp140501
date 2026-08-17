namespace EmotionService.Domain.Entities;

public class ItemType
{
    public int Id { get; private set; }

    public string Name { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    private ItemType() { }

    public static ItemType Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item type name is required.");

        return new ItemType
        {
            Name = name.Trim(),
            IsActive = true
        };
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}