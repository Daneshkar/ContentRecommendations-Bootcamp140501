namespace EmotionService.Application.Features.Experiences.Create;

public sealed record CreateExperienceResponse(
    Guid Id,
    long UserId,
    Guid MediaItemId,
    int Score,
    string? Note,
    IReadOnlyCollection<int> MoodIds,
    IReadOnlyCollection<int> ThemeIds,
    DateTime CreatedAt
);
