using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Music.Delete;
public sealed class DeleteMusicDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<DeleteMusicDetailCommand>
{
    public async Task Handle(DeleteMusicDetailCommand r, CancellationToken ct)
    {
        var e = await db.MusicDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات موسیقی مورد نظر یافت نشد");
        }

        db.MusicDetails.Remove(e);
        await db.SaveChangesAsync(ct);
    }
}
