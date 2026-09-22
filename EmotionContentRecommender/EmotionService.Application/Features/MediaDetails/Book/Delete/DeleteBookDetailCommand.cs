using MediatR;
namespace EmotionService.Application.Features.MediaDetails.Book.Delete;
public sealed record DeleteBookDetailCommand(
    Guid MediaItemId)
    : IRequest;
