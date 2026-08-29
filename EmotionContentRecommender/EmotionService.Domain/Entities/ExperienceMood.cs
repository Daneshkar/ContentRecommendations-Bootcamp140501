namespace EmotionService.Domain.Entities;

public sealed class ExperienceMood
{
    public Guid ExperienceId { get; private set; }

    public Experience Experience { get; private set; } = default!;

    public int MoodId { get; private set; }

    public Mood Mood { get; private set; } = default!;

    public decimal UserWeight { get; private set; }

    private ExperienceMood()
    {
    }

    private ExperienceMood(
        Guid experienceId,
        int moodId,
        decimal userWeight)
    {
        ExperienceId = experienceId;
        MoodId = moodId;
        UserWeight = userWeight;
    }

    public static ExperienceMood Create(
        Guid experienceId,
        int moodId,
        decimal userWeight)
    {
        if (experienceId == Guid.Empty)
        {
            throw new ArgumentException(
                "Experience is required.",
                nameof(experienceId));
        }

        if (moodId <= 0)
        {
            throw new ArgumentException(
                "Mood is required.",
                nameof(moodId));
        }

        ValidateWeight(userWeight);

        return new ExperienceMood(
            experienceId,
            moodId,
            userWeight);
    }

    public void UpdateWeight(decimal userWeight)
    {
        ValidateWeight(userWeight);

        UserWeight = userWeight;
    }

    private static void ValidateWeight(decimal userWeight)
    {
        if (userWeight is < 1.00m or > 5.00m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userWeight),
                "User weight must be between 1.00 and 5.00.");
        }
    }
}