using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Music.GetById;
public sealed record GetMusicDetailByIdQuery(
    Guid MediaItemId)
    : IRequest<MediaDetailResponse>;
