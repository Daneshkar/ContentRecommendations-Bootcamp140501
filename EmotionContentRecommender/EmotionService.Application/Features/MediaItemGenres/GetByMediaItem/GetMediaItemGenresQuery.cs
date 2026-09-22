using MediatR;

namespace EmotionService.Application.Features.MediaItemGenres.GetByMediaItem;

public sealed record GetMediaItemGenresQuery(Guid MediaItemId)
    : IRequest<IReadOnlyList<MediaItemGenreResponse>>;