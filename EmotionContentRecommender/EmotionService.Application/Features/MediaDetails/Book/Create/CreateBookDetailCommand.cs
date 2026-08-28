using MediatR;
using EmotionService.Application.Features.MediaDetails;
namespace EmotionService.Application.Features.MediaDetails.Book.Create;
public sealed record CreateBookDetailCommand(
    Guid MediaItemId,
    string Author,
    string Publisher,
    DateOnly? PublicationDate,
    string Genre,
    string ISBN,
    int? PageCount,
    string Language,
    string Description,
    string? Edition
) : IRequest<MediaDetailResponse>;
