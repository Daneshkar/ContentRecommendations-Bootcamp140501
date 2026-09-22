using MediatR;
namespace EmotionService.Application.Features.MediaDetails.Game.Delete;
public sealed record DeleteGameDetailCommand(
    Guid MediaItemId)
    : IRequest;
