using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Music.GetAll;
public sealed class GetMusicDetailsQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetMusicDetailsQuery, IReadOnlyList<MediaDetailResponse>>
{
    public async Task<IReadOnlyList<MediaDetailResponse>> Handle(
        GetMusicDetailsQuery r,
        CancellationToken ct)
    {
        var details = await db.MusicDetails
            .AsNoTracking()
            .OrderBy(x => x.MediaItemId)
            .ToListAsync(ct);

        return details
            .Select(x => new MediaDetailResponse(x))
            .ToList();
    }
}
