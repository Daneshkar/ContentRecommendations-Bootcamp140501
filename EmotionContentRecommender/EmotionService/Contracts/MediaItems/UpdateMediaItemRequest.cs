namespace EmotionService.Contracts.MediaItems;

public sealed record UpdateMediaItemRequest(
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl
);