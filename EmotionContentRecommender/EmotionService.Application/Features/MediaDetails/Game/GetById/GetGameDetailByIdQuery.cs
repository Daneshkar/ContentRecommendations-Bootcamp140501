using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Game.GetById;
public sealed record GetGameDetailByIdQuery(
    Guid MediaItemId)
    : IRequest<MediaDetailResponse>;
