using MediatR;

namespace EmotionService.Application.Features.MediaItems.Update;

public sealed record UpdateMediaItemCommand(
    Guid Id,
    int ItemTypeId,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? CoverUrl
) : IRequest<UpdateMediaItemResponse>;