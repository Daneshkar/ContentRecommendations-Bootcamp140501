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
        ValidateWeight(weightValue, experienceCount);

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
        ValidateWeight(weightValue, experienceCount);

        WeightValue = weightValue;
        ExperienceCount = experienceCount;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateWeight(
        decimal weightValue,
        int experienceCount)
    {
        if (weightValue is < 0.00m or > 0.10m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weightValue),
                "Weight value must be between 0.00 and 0.10.");
        }

        if (experienceCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(experienceCount),
                "Experience count must be greater than zero.");
        }
    }
}
