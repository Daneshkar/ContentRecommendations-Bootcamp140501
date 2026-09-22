using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.MediaItemGenres.Remove;

public sealed class RemoveGenreFromMediaItemCommandHandler
    : IRequestHandler<RemoveGenreFromMediaItemCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public RemoveGenreFromMediaItemCommandHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        RemoveGenreFromMediaItemCommand request,
        CancellationToken cancellationToken)
    {
        var relation = await _dbContext.MediaItemGenres
            .FirstOrDefaultAsync(
                x => x.MediaItemId == request.MediaItemId &&
                     x.GenreId == request.GenreId,
                cancellationToken);

        if (relation is null)
        {
            throw new NotFoundException(
                "تخصیص ژانر برای این مدیا ایتم پیدا نشد");
        }

        _dbContext.MediaItemGenres.Remove(relation);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}