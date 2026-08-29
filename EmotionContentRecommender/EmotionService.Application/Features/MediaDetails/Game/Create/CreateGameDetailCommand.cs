using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Game.Create;
public sealed record CreateGameDetailCommand(
    Guid MediaItemId,
    string Developer,
    string Publisher,
    int? ReleaseYear,
    string Genre,
    string Platform,
    string Description,
    string? AgeRating,
    string? GameMode,
    string? Engine
) : IRequest<MediaDetailResponse>;
