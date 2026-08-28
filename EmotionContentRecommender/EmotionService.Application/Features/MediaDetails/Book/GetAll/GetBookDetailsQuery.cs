using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Book.GetAll;
public sealed record GetBookDetailsQuery
    : IRequest<IReadOnlyList<MediaDetailResponse>>;
