using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Music.Update;
public sealed class UpdateMusicDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<UpdateMusicDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        UpdateMusicDetailCommand r,
        CancellationToken ct)
    {
        var e = await db.MusicDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات موسیقی مورد نظر یافت نشد");
        }

        e.Update(
            r.Artist,
            r.Album,
            r.ReleaseYear,
            r.Genre,
            r.DurationSeconds,
            r.TrackNumber,
            r.Description,
            r.Publisher,
            r.Language);

        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
