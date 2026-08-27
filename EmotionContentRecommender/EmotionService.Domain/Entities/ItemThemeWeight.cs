namespace EmotionService.Domain.Entities;

public class ItemThemeWeight
{
    public Guid Id { get; private set; }

    public Guid MediaItemId { get; private set; }

    public MediaItem MediaItem { get; private set; } = default!;

    public int ThemeId { get; private set; }

    public Theme Theme { get; private set; } = default!;

    public decimal WeightValue { get; private set; }

    public int ExperienceCount { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private ItemThemeWeight()
    {
    }

    public static ItemThemeWeight Create(
        Guid mediaItemId,
        int themeId,
        decimal weightValue,
        int experienceCount)
    {
        return new ItemThemeWeight
        {
            Id = Guid.NewGuid(),
            MediaItemId = mediaItemId,
            ThemeId = themeId,
            WeightValue = weightValue,
            ExperienceCount = experienceCount,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateWeight(
        decimal weightValue,
        int experienceCount)
    {
        WeightValue = weightValue;
        ExperienceCount = experienceCount;
        UpdatedAt = DateTime.UtcNow;
    }
}