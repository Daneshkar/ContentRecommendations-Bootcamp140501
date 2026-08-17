using MediatR;

namespace EmotionService.Application.Features.MediaItemGenres.Remove;

public sealed record RemoveGenreFromMediaItemCommand(
    Guid MediaItemId,
    int GenreId
) : IRequest;