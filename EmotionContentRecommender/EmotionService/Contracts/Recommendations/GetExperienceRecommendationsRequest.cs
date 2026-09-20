namespace EmotionService.Contracts.Recommendations;

public sealed record GetExperienceRecommendationsRequest(
    Guid ExperienceId,
    int? PageSize,
    string? Cursor);
