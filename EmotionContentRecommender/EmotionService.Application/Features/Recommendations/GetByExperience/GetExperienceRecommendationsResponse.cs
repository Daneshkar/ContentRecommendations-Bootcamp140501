using EmotionService.Application.Features.Recommendations.Get;

namespace EmotionService.Application.Features.Recommendations.GetByExperience;

public sealed record GetExperienceRecommendationsResponse(
    ExperienceRecommendationCriteriaResponse Criteria,
    IReadOnlyList<RecommendationItemResponse> Items,
    string? NextCursor,
    bool HasMore,
    string? EmptyReason);

public sealed record ExperienceRecommendationCriteriaResponse(
    Guid ExperienceId,
    ExperienceRecommendationSourceItemResponse SourceMediaItem,
    RecommendationNamedValueResponse ItemType,
    IReadOnlyList<RecommendationNamedValueResponse> Moods,
    IReadOnlyList<RecommendationNamedValueResponse> Themes);

public sealed record ExperienceRecommendationSourceItemResponse(
    Guid Id,
    string Name);
