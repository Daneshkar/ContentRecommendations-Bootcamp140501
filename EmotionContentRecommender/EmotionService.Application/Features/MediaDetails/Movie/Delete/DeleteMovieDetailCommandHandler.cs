using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EmotionService.Application.Features.MediaDetails.Movie.Delete;
public sealed class DeleteMovieDetailCommandHandler(ApplicationDbContext db)
    : IRequestHandler<DeleteMovieDetailCommand>
{
    public async Task Handle(DeleteMovieDetailCommand r, CancellationToken ct)
    {
        var e = await db.MovieDetails
            .FirstOrDefaultAsync(x => x.MediaItemId == r.MediaItemId, ct);

        if (e is null)
        {
            throw new NotFoundException("جزئیات فیلم مورد نظر یافت نشد");
        }

        db.MovieDetails.Remove(e);
        await db.SaveChangesAsync(ct);
    }
}
