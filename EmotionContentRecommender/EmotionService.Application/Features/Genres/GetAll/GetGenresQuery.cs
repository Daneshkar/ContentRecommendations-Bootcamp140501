using MediatR;

namespace EmotionService.Application.Features.Genres.GetAll;

public sealed record GetGenresQuery(
    int? ItemTypeId = null
) : IRequest<IReadOnlyList<GenreListItemResponse>>;