using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Music.Update;
public sealed record UpdateMusicDetailCommand(
    Guid MediaItemId,
    string Artist,
    string? Album,
    int? ReleaseYear,
    string Genre,
    int DurationSeconds,
    int? TrackNumber,
    string? Description,
    string? Publisher,
    string? Language
) : IRequest<MediaDetailResponse>;
