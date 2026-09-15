namespace EmotionService.Domain.Entities;

public sealed class ExperienceTheme
{
    public Guid ExperienceId { get; private set; }

    public Experience Experience { get; private set; } = default!;

    public int ThemeId { get; private set; }

    public Theme Theme { get; private set; } = default!;

    public decimal UserWeight { get; private set; }

    private ExperienceTheme()
    {
    }

    private ExperienceTheme(
        Guid experienceId,
        int themeId,
        decimal userWeight)
    {
        ExperienceId = experienceId;
        ThemeId = themeId;
        UserWeight = userWeight;
    }

    public static ExperienceTheme Create(
        Guid experienceId,
        int themeId,
        decimal userWeight)
    {
        if (experienceId == Guid.Empty)
        {
            throw new ArgumentException(
                "Experience is required.",
                nameof(experienceId));
        }

        if (themeId <= 0)
        {
            throw new ArgumentException(
                "Theme is required.",
                nameof(themeId));
        }

        ValidateWeight(userWeight);

        return new ExperienceTheme(
            experienceId,
            themeId,
            userWeight);
    }

    public void UpdateWeight(decimal userWeight)
    {
        ValidateWeight(userWeight);

        UserWeight = userWeight;
    }

    private static void ValidateWeight(decimal userWeight)
    {
        if (userWeight is < 0.00m or > 0.10m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userWeight),
                "User weight must be between 0.00 and 0.10.");
        }
    }
}
