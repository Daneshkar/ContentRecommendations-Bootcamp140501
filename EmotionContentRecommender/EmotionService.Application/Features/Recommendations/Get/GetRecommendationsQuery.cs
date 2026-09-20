using MediatR;

namespace EmotionService.Application.Features.Recommendations.Get;

public sealed record GetRecommendationsQuery(
    long UserId,
    int ItemTypeId,
    int PrimaryMoodId,
    IReadOnlyCollection<int> AdditionalMoodIds,
    IReadOnlyCollection<int> ThemeIds,
    int PageSize,
    string? Cursor)
    : IRequest<GetRecommendationsResponse>;
