namespace EmotionService.Application.Features.MediaItems.GetById;

public sealed record GetMediaItemByIdResponse(
    Guid Id,
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);