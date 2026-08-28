using EmotionService.Application.Features.MediaDetails;
using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Game.Create;
public sealed class CreateGameDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<CreateGameDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        CreateGameDetailCommand r,
        CancellationToken ct)
    {
        if (!await db.MediaItems.AnyAsync(x => x.Id == r.MediaItemId, ct))
        {
            throw new NotFoundException("مدیا آیتم مورد نظر یافت نشد");
        }

        if (await db.GameDetails.AnyAsync(x => x.MediaItemId == r.MediaItemId, ct))
        {
            throw new ConflictException("جزئیات بازی برای این مدیا آیتم وجود دارد");
        }

        var e = GameDetail.Create(
            r.MediaItemId,
            r.Developer,
            r.Publisher,
            r.ReleaseYear,
            r.Genre,
            r.Platform,
            r.Description,
            r.AgeRating,
            r.GameMode,
            r.Engine);

        db.GameDetails.Add(e);
        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
