using EmotionService.Infrastructure.Exceptions;
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
            throw new NotFoundException("ژانر مورد نظر یافت نشد");

        var itemTypeExists = await _dbContext.ItemTypes
            .AnyAsync(
                x => x.Id == request.ItemTypeId,
                cancellationToken);

        if (!itemTypeExists)
            throw new NotFoundException("مدیا تایپ مورد نظر موجود نیست");

        var duplicateExists = await _dbContext.Genres
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.ItemTypeId == request.ItemTypeId &&
                     x.Name == request.Name,
                cancellationToken);

        if (duplicateExists)
            throw new ConflictException(
                "این ژانر برای مدیا تایپ مورد نظر وجود دارد");

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