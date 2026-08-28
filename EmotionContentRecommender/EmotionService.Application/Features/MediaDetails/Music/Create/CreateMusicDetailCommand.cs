using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Music.Create;
public sealed record CreateMusicDetailCommand(
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
