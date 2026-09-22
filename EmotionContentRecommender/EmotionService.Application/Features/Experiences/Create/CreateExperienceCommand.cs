using MediatR;

namespace EmotionService.Application.Features.Experiences.Create;

public sealed record CreateExperienceCommand(
    long UserId,
    Guid MediaItemId,
    int Score,
    string? Note,
    IReadOnlyCollection<int> MoodIds,
    IReadOnlyCollection<int> ThemeIds
) : IRequest<CreateExperienceResponse>;
