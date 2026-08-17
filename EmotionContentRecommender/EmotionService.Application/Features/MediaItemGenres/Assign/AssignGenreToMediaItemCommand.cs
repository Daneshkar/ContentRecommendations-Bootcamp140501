using MediatR;

namespace EmotionService.Application.Features.MediaItemGenres.Assign;

public sealed record AssignGenreToMediaItemCommand(
    Guid MediaItemId,
    int GenreId
) : IRequest;