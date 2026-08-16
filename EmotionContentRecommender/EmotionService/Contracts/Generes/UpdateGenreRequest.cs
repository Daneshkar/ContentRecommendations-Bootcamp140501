namespace EmotionService.Contracts.Genres;

public sealed record UpdateGenreRequest(
    int ItemTypeId,
    string Name,
    string? Description
);