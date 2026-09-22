using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Book.Update;
public sealed class UpdateBookDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<UpdateBookDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        UpdateBookDetailCommand r,
        CancellationToken ct)
    {
        var e = await db.BookDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات کتاب مورد نظر یافت نشد");
        }

        e.Update(
            r.Author,
            r.Publisher,
            r.PublicationDate,
            r.Genre,
            r.ISBN,
            r.PageCount,
            r.Language,
            r.Description,
            r.Edition);

        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
