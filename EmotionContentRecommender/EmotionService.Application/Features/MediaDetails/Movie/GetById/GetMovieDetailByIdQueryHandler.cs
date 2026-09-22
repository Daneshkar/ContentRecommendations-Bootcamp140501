using EmotionService.Application.Features.MediaDetails;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Movie.GetById;
public sealed class GetMovieDetailByIdQueryHandler(ApplicationDbContext db)
    : IRequestHandler<GetMovieDetailByIdQuery, MediaDetailResponse>
{
    public async Task<MediaDetailResponse> Handle(
        GetMovieDetailByIdQuery r,
        CancellationToken ct)
    {
        var e = await db.MovieDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات فیلم مورد نظر یافت نشد");
        }

        return new(e);
    }
}
