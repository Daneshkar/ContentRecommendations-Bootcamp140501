namespace EmotionService.Contracts.Experiences;

public sealed record UpdateExperienceRequest(
    int Score,
    string? Note,
    IReadOnlyCollection<int>? MoodIds,
    IReadOnlyCollection<int>? ThemeIds
);
