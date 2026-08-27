namespace EmotionService.Domain.Entities;

public class Mood
{
    public int Id { get; set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Mood() { }

    private Mood(string name, string? description)
    {
        Name = name.Trim();
        Description = NormalizeDescription(description);
        IsActive = true;
    }

    public static Mood Create(
    string name,
    string? description = null)
    {
        return new Mood(name, description);
    }

    public void Update(
        string name,
        string? description)
    {
        Name = name.Trim();
        Description = NormalizeDescription(description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}
