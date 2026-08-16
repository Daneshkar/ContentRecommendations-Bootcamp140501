namespace EmotionService.Application.Features.Genres.GetById;

public sealed record GetGenreByIdResponse(
    int Id,
    int ItemTypeId,
    string Name,
    string? Description
);