namespace EmotionService.Application.Features.MediaItems.GetAll;

public sealed record MediaItemListItemResponse(
    Guid Id,
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl,
    bool IsActive
);