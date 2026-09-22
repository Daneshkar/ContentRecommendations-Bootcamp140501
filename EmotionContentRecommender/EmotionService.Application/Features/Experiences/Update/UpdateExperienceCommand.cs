using MediatR;

namespace EmotionService.Application.Features.Experiences.Update;

public sealed record UpdateExperienceCommand(
    Guid Id,
    long UserId,
    int Score,
    string? Note,
    IReadOnlyCollection<int> MoodIds,
    IReadOnlyCollection<int> ThemeIds
) : IRequest<UpdateExperienceResponse>;
