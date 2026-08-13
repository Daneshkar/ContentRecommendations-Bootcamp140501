namespace EmotionService.Contracts.MediaItems;

public sealed record CreateMediaItemRequest(
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl
);