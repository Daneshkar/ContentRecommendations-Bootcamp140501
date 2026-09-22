using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Game.Delete;
public sealed class DeleteGameDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<DeleteGameDetailCommand>
{
    public async Task Handle(DeleteGameDetailCommand r, CancellationToken ct)
    {
        var e = await db.GameDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات بازی مورد نظر یافت نشد");
        }

        db.GameDetails.Remove(e);
        await db.SaveChangesAsync(ct);
    }
}
