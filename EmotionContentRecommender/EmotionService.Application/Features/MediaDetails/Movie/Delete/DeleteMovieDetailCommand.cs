using MediatR;
namespace EmotionService.Application.Features.MediaDetails.Movie.Delete;
public sealed record DeleteMovieDetailCommand(
    Guid MediaItemId)
    : IRequest;
