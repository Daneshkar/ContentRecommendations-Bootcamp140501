using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Movie.GetAll;
public sealed record GetMovieDetailsQuery
    : IRequest<IReadOnlyList<MediaDetailResponse>>;
