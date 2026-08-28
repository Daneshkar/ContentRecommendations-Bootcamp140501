using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Movie.GetById;
public sealed record GetMovieDetailByIdQuery(
    Guid MediaItemId)
    : IRequest<MediaDetailResponse>;
