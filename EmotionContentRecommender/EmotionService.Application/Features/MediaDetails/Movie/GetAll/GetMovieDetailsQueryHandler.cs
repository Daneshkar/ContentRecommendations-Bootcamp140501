using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Movie.GetAll;
public sealed class GetMovieDetailsQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetMovieDetailsQuery, IReadOnlyList<MediaDetailResponse>>
{
    public async Task<IReadOnlyList<MediaDetailResponse>> Handle(
        GetMovieDetailsQuery r,
        CancellationToken ct)
    {
        var details = await db.MovieDetails
            .AsNoTracking()
            .OrderBy(x => x.MediaItemId)
            .ToListAsync(ct);

        return details
            .Select(x => new MediaDetailResponse(x))
            .ToList();
    }
}
