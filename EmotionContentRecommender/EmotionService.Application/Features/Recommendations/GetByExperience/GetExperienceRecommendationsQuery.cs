using MediatR;

namespace EmotionService.Application.Features.Recommendations.GetByExperience;

public sealed record GetExperienceRecommendationsQuery(
    long UserId,
    Guid ExperienceId,
    int PageSize,
    string? Cursor)
    : IRequest<GetExperienceRecommendationsResponse>;
