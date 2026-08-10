namespace EmotionService.Domain.Entities;
public class Mood 
{
    public long Id { get; set; }
    public string  Name        { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool    IsActive    { get; private set; } = true;

    private Mood() { }

    public static Mood Create(string name, string? description = null)
        => new()
        {
            Name        = name.Trim(),
            Description = description,
            IsActive    = true
        };

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;
}
