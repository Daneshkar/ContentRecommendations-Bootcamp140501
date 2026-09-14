namespace EmotionService.Contracts.Experiences;

public sealed record CreateExperienceRequest(
    Guid MediaItemId,
    int Score,
    string? Note,
    IReadOnlyCollection<int>? MoodIds,
    IReadOnlyCollection<int>? ThemeIds
);
