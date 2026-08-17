namespace EmotionService.Application.Features.Genres.Update;

public sealed record UpdateGenreResponse(
    int Id,
    int ItemTypeId,
    string Name,
    string? Description
);