using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.MediaItemGenres.GetByMediaItem;

public sealed class GetMediaItemGenresQueryHandler
    : IRequestHandler<
        GetMediaItemGenresQuery,
        IReadOnlyList<MediaItemGenreResponse>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMediaItemGenresQueryHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MediaItemGenreResponse>> Handle(
        GetMediaItemGenresQuery request,
        CancellationToken cancellationToken)
    {
        var mediaItemExists = await _dbContext.MediaItems
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.MediaItemId,
                cancellationToken);

        if (!mediaItemExists)
            throw new NotFoundException("مدیا آیتم مورد نظر یافت نشد");

        return await _dbContext.MediaItemGenres
            .AsNoTracking()
            .Where(x => x.MediaItemId == request.MediaItemId)
            .OrderBy(x => x.Genre.Name)
            .Select(x => new MediaItemGenreResponse(
                x.Genre.Id,
                x.Genre.Name,
                x.Genre.Description
            ))
            .ToListAsync(cancellationToken);
    }
}