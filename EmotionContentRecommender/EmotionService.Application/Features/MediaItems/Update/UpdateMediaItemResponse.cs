namespace EmotionService.Application.Features.MediaItems.Update;

public sealed record UpdateMediaItemResponse(
    Guid Id,
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl,
    bool IsActive,
    DateTime UpdatedAt
);