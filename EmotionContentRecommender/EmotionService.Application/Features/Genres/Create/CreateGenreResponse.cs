namespace EmotionService.Application.Features.Genres.Create;

public sealed record CreateGenreResponse(
    int Id,
    int ItemTypeId,
    string Name,
    string? Description
);