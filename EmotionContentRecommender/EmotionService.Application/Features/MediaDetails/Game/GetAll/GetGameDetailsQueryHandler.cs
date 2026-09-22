using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Game.GetAll;
public sealed class GetGameDetailsQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetGameDetailsQuery, IReadOnlyList<MediaDetailResponse>>
{
    public async Task<IReadOnlyList<MediaDetailResponse>> Handle(
        GetGameDetailsQuery r,
        CancellationToken ct)
    {
        var details = await db.GameDetails
            .AsNoTracking()
            .OrderBy(x => x.MediaItemId)
            .ToListAsync(ct);

        return details
            .Select(x => new MediaDetailResponse(x))
            .ToList();
    }
}
