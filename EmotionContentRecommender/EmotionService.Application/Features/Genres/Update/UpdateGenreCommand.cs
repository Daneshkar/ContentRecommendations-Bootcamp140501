using MediatR;

namespace EmotionService.Application.Features.Genres.Update;

public sealed record UpdateGenreCommand(
    int Id,
    int ItemTypeId,
    string Name,
    string? Description
) : IRequest<UpdateGenreResponse>;