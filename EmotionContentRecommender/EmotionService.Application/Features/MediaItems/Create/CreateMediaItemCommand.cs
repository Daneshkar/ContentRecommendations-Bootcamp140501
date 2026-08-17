using MediatR;

namespace EmotionService.Application.Features.MediaItems.Create;

public sealed record CreateMediaItemCommand(
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl
) : IRequest<CreateMediaItemResponse>;