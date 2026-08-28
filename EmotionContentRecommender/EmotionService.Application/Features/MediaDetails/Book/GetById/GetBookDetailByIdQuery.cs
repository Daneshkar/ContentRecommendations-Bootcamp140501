using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Book.GetById;
public sealed record GetBookDetailByIdQuery(
    Guid MediaItemId)
    : IRequest<MediaDetailResponse>;
