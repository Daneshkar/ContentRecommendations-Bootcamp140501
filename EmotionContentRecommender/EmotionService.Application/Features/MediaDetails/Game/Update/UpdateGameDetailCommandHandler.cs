using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Game.Update;
public sealed class UpdateGameDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<UpdateGameDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        UpdateGameDetailCommand r,
        CancellationToken ct)
    {
        var e = await db.GameDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات بازی مورد نظر یافت نشد");
        }

        e.Update(
            r.Developer,
            r.Publisher,
            r.ReleaseYear,
            r.Genre,
            r.Platform,
            r.Description,
            r.AgeRating,
            r.GameMode,
            r.Engine);

        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
