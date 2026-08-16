using MediatR;

namespace EmotionService.Application.Features.Genres.Create;

public sealed record CreateGenreCommand(
    int ItemTypeId,
    string Name,
    string? Description
) : IRequest<CreateGenreResponse>;