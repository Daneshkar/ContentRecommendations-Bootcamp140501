using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.MediaItemGenres.Assign;

public sealed class AssignGenreToMediaItemCommandHandler
    : IRequestHandler<AssignGenreToMediaItemCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public AssignGenreToMediaItemCommandHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        AssignGenreToMediaItemCommand request,
        CancellationToken cancellationToken)
    {
        var mediaItem = await _dbContext.MediaItems
            .FirstOrDefaultAsync(
                x => x.Id == request.MediaItemId,
                cancellationToken);

        if (mediaItem is null)
            throw new NotFoundException("مدیا ایتم مورد نظر یافت نشد");

        var genre = await _dbContext.Genres
            .FirstOrDefaultAsync(
                x => x.Id == request.GenreId,
                cancellationToken);

        if (genre is null)
            throw new NotFoundException("ژانر مورد نظر یافت نشد");

        if (mediaItem.ItemTypeId != genre.ItemTypeId)
        {
            throw new BadRequestException(
                "ژانر مورد نظر متعلق به این نوع از تایپ این مدیا آیتم نیست");
        }

        var alreadyAssigned = await _dbContext.MediaItemGenres
            .AnyAsync(
                x => x.MediaItemId == request.MediaItemId &&
                     x.GenreId == request.GenreId,
                cancellationToken);

        if (alreadyAssigned)
            return;

        var mediaItemGenre = MediaItemGenre.Create(
            request.MediaItemId,
            request.GenreId);

        _dbContext.MediaItemGenres.Add(mediaItemGenre);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}