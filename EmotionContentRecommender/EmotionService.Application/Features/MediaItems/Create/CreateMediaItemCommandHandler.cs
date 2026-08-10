using EmotionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EmotionService.Infrastructure.Persistence;
using MediatR;

namespace EmotionService.Application.Features.MediaItems.Create;

public sealed class CreateMediaItemCommandHandler : IRequestHandler<CreateMediaItemCommand, CreateMediaItemResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateMediaItemCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateMediaItemResponse> Handle(
        CreateMediaItemCommand command,
        CancellationToken cancellationToken)
    {
        var itemTypeExists = await _dbContext.ItemTypes
            .AnyAsync(
                x => x.Id == command.ItemTypeId,
                cancellationToken);

        if (!itemTypeExists)
            throw new ArgumentException("Item type does not exist.");

        var mediaItem = MediaItem.Create(
            command.Name,
            command.Description,
            command.ItemTypeId,
            command.ReleaseDate,
            command.CoverUrl);

        _dbContext.MediaItems.Add(mediaItem);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateMediaItemResponse(
                    mediaItem.Id,
                    mediaItem.Name,
                    mediaItem.ItemTypeId,
                    mediaItem.Description,
                    mediaItem.ReleaseDate,
                    mediaItem.ImageUrl,
                    mediaItem.Status,
                    mediaItem.CreatedAt
                );
    }
}