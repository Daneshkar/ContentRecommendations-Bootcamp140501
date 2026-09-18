namespace EmotionService.Domain.Entities;

public class ItemMoodWeight
{
    public Guid Id { get; private set; }

    public Guid MediaItemId { get; private set; }

    public MediaItem MediaItem { get; private set; } = default!;

    public int MoodId { get; private set; }

    public Mood Mood { get; private set; } = default!;

    public decimal WeightValue { get; private set; }

    public int ExperienceCount { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private ItemMoodWeight()
    {
    }

    public static ItemMoodWeight Create(
        Guid mediaItemId,
        int moodId,
        decimal weightValue,
        int experienceCount)
    {
        ValidateWeight(weightValue, experienceCount);

        return new ItemMoodWeight
        {
            Id = Guid.NewGuid(),
            MediaItemId = mediaItemId,
            MoodId = moodId,
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
