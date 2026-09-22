using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Music.GetById;
public sealed class GetMusicDetailByIdQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetMusicDetailByIdQuery, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        GetMusicDetailByIdQuery r,
        CancellationToken ct)
    {
        var e = await db.MusicDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات موسیقی مورد نظر یافت نشد");
        }

        return new(e);
    }
}
