namespace EmotionService.Application.Features.MediaItems.Create;

public sealed record CreateMediaItemResponse(
    Guid Id,
    string Name,
    int ItemTypeId,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl,
    bool Status,
    DateTime CreatedAt
);