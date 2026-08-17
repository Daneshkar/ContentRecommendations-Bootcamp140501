namespace EmotionService.Application.Features.Genres.GetAll;

public sealed record GenreListItemResponse(
    int Id,
    int ItemTypeId,
    string Name,
    string? Description
);