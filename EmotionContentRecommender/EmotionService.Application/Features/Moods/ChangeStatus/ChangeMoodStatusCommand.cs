using MediatR;

namespace EmotionService.Application.Features.Moods.ChangeStatus;

public sealed record ChangeMoodStatusCommand(
    int Id,
    bool IsActive
) : IRequest<ChangeMoodStatusResponse>;