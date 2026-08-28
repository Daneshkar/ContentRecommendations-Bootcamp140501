using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Game.GetById;
public sealed class GetGameDetailByIdQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetGameDetailByIdQuery, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        GetGameDetailByIdQuery r,
        CancellationToken ct)
    {
        var e = await db.GameDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات بازی مورد نظر یافت نشد");
        }

        return new(e);
    }
}
