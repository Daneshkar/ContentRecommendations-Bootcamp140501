using EmotionService.Application.Features.MediaDetails;
using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Music.Create;
public sealed class CreateMusicDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<CreateMusicDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        CreateMusicDetailCommand r,
        CancellationToken ct)
    {
        if (!await db.MediaItems.AnyAsync(x => x.Id == r.MediaItemId, ct))
        {
            throw new NotFoundException("مدیا آیتم مورد نظر یافت نشد");
        }

        if (await db.MusicDetails.AnyAsync(x => x.MediaItemId == r.MediaItemId, ct))
        {
            throw new ConflictException("جزئیات موسیقی برای این مدیا آیتم وجود دارد");
        }

        var e = MusicDetail.Create(
            r.MediaItemId,
            r.Artist,
            r.Album,
            r.ReleaseYear,
            r.Genre,
            r.DurationSeconds,
            r.TrackNumber,
            r.Description,
            r.Publisher,
            r.Language);

        db.MusicDetails.Add(e);
        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
