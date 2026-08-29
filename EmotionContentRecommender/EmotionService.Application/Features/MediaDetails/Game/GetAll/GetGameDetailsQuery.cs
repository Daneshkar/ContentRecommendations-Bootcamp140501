using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Game.GetAll;
public sealed record GetGameDetailsQuery
    : IRequest<IReadOnlyList<MediaDetailResponse>>;
