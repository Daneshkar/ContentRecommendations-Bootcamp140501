using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Music.GetAll;
public sealed record GetMusicDetailsQuery
    : IRequest<IReadOnlyList<MediaDetailResponse>>;
