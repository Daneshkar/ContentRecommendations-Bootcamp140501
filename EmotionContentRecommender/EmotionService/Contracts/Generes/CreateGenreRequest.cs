namespace EmotionService.Contracts.Genres;

public sealed record CreateGenreRequest(
    int ItemTypeId,
    string Name,
    string? Description
);