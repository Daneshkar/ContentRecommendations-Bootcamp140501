namespace EmotionService.Application.Features.MediaItemGenres.GetByMediaItem;

public sealed record MediaItemGenreResponse(
    int Id,
    string Name,
    string? Description
);