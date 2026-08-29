using EmotionService.Application.Features.MediaDetails;
using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Movie.Create;
public sealed class CreateMovieDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<CreateMovieDetailCommand, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        CreateMovieDetailCommand r,
        CancellationToken ct)
    {
        if (!await db.MediaItems.AnyAsync(x => x.Id == r.MediaItemId, ct))
        {
            throw new NotFoundException("مدیا آیتم مورد نظر یافت نشد");
        }

        if (await db.MovieDetails.AnyAsync(x => x.MediaItemId == r.MediaItemId, ct))
        {
            throw new ConflictException("جزئیات فیلم برای این مدیا آیتم وجود دارد");
        }

        var e = MovieDetail.Create(
            r.MediaItemId,
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

        db.MovieDetails.Add(e);
        await db.SaveChangesAsync(ct);

        return new(e);
    }
}
