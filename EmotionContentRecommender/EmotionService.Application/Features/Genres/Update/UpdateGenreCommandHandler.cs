using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Genres.Update;

public sealed class UpdateGenreCommandHandler
    : IRequestHandler<UpdateGenreCommand, UpdateGenreResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateGenreCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateGenreResponse> Handle(
        UpdateGenreCommand request,
        CancellationToken cancellationToken)
    {
        var genre = await _dbContext.Genres
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (genre is null)
            throw new KeyNotFoundException("Genre not found.");

        var itemTypeExists = await _dbContext.ItemTypes
            .AnyAsync(
                x => x.Id == request.ItemTypeId,
                cancellationToken);

        if (!itemTypeExists)
            throw new ArgumentException("Item type does not exist.");

        var duplicateExists = await _dbContext.Genres
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.ItemTypeId == request.ItemTypeId &&
                     x.Name == request.Name,
                cancellationToken);

        if (duplicateExists)
            throw new ArgumentException(
                "Genre already exists for this item type.");

        genre.Update(
            request.ItemTypeId,
            request.Name,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateGenreResponse(
            genre.Id,
            genre.ItemTypeId,
            genre.Name,
            genre.Description);
    }
}