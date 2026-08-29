using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Book.Delete;
public sealed class DeleteBookDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<DeleteBookDetailCommand>
{
    public async Task Handle(DeleteBookDetailCommand r, CancellationToken ct)
    {
        var e = await db.BookDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات کتاب مورد نظر یافت نشد");
        }

        db.BookDetails.Remove(e);
        await db.SaveChangesAsync(ct);
    }
}
