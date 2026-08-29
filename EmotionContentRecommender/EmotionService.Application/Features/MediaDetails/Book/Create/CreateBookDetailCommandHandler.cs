using EmotionService.Application.Features.MediaDetails;
using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Book.Create;
public sealed class CreateBookDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<CreateBookDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        CreateBookDetailCommand r,
        CancellationToken ct)
    {
        if (!await db.MediaItems.AnyAsync(x => x.Id == r.MediaItemId, ct))
        {
            throw new NotFoundException("مدیا آیتم مورد نظر یافت نشد");
        }

        if (await db.BookDetails.AnyAsync(x => x.MediaItemId == r.MediaItemId, ct))
        {
            throw new ConflictException("جزئیات کتاب برای این مدیا آیتم وجود دارد");
        }

        var e = BookDetail.Create(
            r.MediaItemId,
            r.Author,
            r.Publisher,
            r.PublicationDate,
            r.Genre,
            r.ISBN,
            r.PageCount,
            r.Language,
            r.Description,
            r.Edition);

        db.BookDetails.Add(e);
        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
