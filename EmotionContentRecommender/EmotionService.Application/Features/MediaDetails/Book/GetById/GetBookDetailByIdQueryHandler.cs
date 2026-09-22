using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Book.GetById;
public sealed class GetBookDetailByIdQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetBookDetailByIdQuery, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        GetBookDetailByIdQuery r,
        CancellationToken ct)
    {
        var e = await db.BookDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات کتاب مورد نظر یافت نشد");
        }

        return new(e);
    }
}
