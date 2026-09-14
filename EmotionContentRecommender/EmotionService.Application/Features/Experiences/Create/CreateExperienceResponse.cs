namespace EmotionService.Application.Features.Experiences.Create;

public sealed record CreateExperienceResponse(
    Guid Id,
    long UserId,
    Guid MediaItemId,
    int Score,
    string? Note,
    decimal UserWeight,
    IReadOnlyCollection<int> MoodIds,
    IReadOnlyCollection<int> ThemeIds,
    DateTime CreatedAt
);
