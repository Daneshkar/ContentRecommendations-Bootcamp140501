using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Book.GetAll;
public sealed class GetBookDetailsQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetBookDetailsQuery, IReadOnlyList<MediaDetailResponse>>
{
    public async Task<IReadOnlyList<MediaDetailResponse>> Handle(
        GetBookDetailsQuery r,
        CancellationToken ct)
    {
        var details = await db.BookDetails
            .AsNoTracking()
            .OrderBy(x => x.MediaItemId)
            .ToListAsync(ct);

        return details
            .Select(x => new MediaDetailResponse(x))
            .ToList();
    }
}
