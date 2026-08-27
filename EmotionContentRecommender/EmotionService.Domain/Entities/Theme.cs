namespace EmotionService.Domain.Entities;

public sealed class Theme
{
    public int Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    private Theme()
    {
    }

    private Theme(string name, string? description)
    {
        Name = name.Trim();
        Description = NormalizeDescription(description);
        IsActive = true;
    }

    public static Theme Create(
        string name,
        string? description = null)
    {
        return new Theme(name, description);
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