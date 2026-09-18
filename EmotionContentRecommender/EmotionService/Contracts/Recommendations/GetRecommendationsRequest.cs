namespace EmotionService.Contracts.Recommendations;

public sealed record GetRecommendationsRequest(
    int ItemTypeId,
    int PrimaryMoodId,
    IReadOnlyCollection<int>? AdditionalMoodIds,
    IReadOnlyCollection<int>? ThemeIds,
    int? PageSize,
    string? Cursor);
