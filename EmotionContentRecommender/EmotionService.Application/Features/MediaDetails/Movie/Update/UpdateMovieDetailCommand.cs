using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Movie.Update;
public sealed record UpdateMovieDetailCommand(
    Guid MediaItemId,
    string Director,
    int? ReleaseYear,
    int DurationMinutes,
    string Genre,
    string Synopsis,
    string? Language,
    string? Country,
    string? AgeRating,
    string? Cast,
    string? Studio
) : IRequest<MediaDetailResponse>;
