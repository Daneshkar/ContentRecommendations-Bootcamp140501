using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.MediaItems.Update;

public sealed class UpdateMediaItemCommandHandler
    : IRequestHandler<UpdateMediaItemCommand, UpdateMediaItemResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateMediaItemCommandHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateMediaItemResponse> Handle(
        UpdateMediaItemCommand request,
        CancellationToken cancellationToken)
    {
        var mediaItem = await _dbContext.MediaItems
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (mediaItem is null)
            throw new KeyNotFoundException("Media item not found.");

        var itemTypeExists = await _dbContext.ItemTypes
            .AnyAsync(
                x => x.Id == request.ItemTypeId,
                cancellationToken);

        if (!itemTypeExists)
            throw new ArgumentException("Item type does not exist.");

        mediaItem.Update(
            request.ItemTypeId,
            request.Name,
            request.Description,
            request.ReleaseDate,
            request.CoverUrl);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateMediaItemResponse(
            mediaItem.Id,
            mediaItem.ItemTypeId,
            mediaItem.Name,
            mediaItem.Description,
            mediaItem.ReleaseDate,
            mediaItem.ImageUrl,
            mediaItem.Status,
            mediaItem.UpdatedAt!.Value
        );
    }
}