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
        WeightValue = weightValue;
        ExperienceCount = experienceCount;
        UpdatedAt = DateTime.UtcNow;
    }
}