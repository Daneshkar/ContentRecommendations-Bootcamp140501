using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Genres.Create;

public sealed class CreateGenreCommandHandler
    : IRequestHandler<CreateGenreCommand, CreateGenreResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateGenreCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateGenreResponse> Handle(
        CreateGenreCommand request,
        CancellationToken cancellationToken)
    {
        var itemTypeExists = await _dbContext.ItemTypes
            .AnyAsync(
                x => x.Id == request.ItemTypeId,
                cancellationToken);

        if (!itemTypeExists)
            throw new ArgumentException("Item type does not exist.");

        var duplicateExists = await _dbContext.Genres
            .AnyAsync(
                x => x.ItemTypeId == request.ItemTypeId &&
                     x.Name == request.Name,
                cancellationToken);

        if (duplicateExists)
            throw new ArgumentException(
                "Genre already exists for this item type.");

        var genre = Genre.Create(
            request.ItemTypeId,
            request.Name,
            request.Description);

        _dbContext.Genres.Add(genre);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateGenreResponse(
            genre.Id,
            genre.ItemTypeId,
            genre.Name,
            genre.Description);
    }
}