using MediatR;

namespace EmotionService.Application.Features.MediaItems.Deactivate;

public sealed record DeactivateMediaItemCommand(Guid Id)
    : IRequest;