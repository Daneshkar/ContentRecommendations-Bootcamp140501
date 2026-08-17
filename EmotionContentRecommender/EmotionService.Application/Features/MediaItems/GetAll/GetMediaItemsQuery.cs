using MediatR;

namespace EmotionService.Application.Features.MediaItems.GetAll;

public sealed record GetMediaItemsQuery(
    int? ItemTypeId = null,
    string? Search = null
) : IRequest<IReadOnlyList<MediaItemListItemResponse>>;