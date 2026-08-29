using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Movie.Update;
public sealed class UpdateMovieDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<UpdateMovieDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        UpdateMovieDetailCommand r,
        CancellationToken ct)
    {
        var e = await db.MovieDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات فیلم مورد نظر یافت نشد");
        }

        e.Update(
            r.Director,
            r.ReleaseYear,
            r.DurationMinutes,
            r.Genre,
            r.Synopsis,
            r.Language,
            r.Country,
            r.AgeRating,
            r.Cast,
            r.Studio);

        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
