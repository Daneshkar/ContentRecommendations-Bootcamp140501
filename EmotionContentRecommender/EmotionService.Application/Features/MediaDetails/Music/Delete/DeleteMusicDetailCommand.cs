using MediatR;
namespace EmotionService.Application.Features.MediaDetails.Music.Delete;
public sealed record DeleteMusicDetailCommand(
    Guid MediaItemId)
    : IRequest;
