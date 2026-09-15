namespace EmotionService.Application.Features.Experiences.Update;

public sealed record UpdateExperienceResponse(
    Guid Id,
    Guid MediaItemId,
    int Score,
    string? Note,
    IReadOnlyCollection<int> MoodIds,
    IReadOnlyCollection<int> ThemeIds,
    DateTime? UpdatedAt
);
