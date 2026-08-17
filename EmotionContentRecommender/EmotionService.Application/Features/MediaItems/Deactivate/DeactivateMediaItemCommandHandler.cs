using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.MediaItems.Deactivate;

public sealed class DeactivateMediaItemCommandHandler
    : IRequestHandler<DeactivateMediaItemCommand>
{
    private readonly ApplicationDbContext _dbContext;

    public DeactivateMediaItemCommandHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        DeactivateMediaItemCommand request,
        CancellationToken cancellationToken)
    {
        var mediaItem = await _dbContext.MediaItems
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (mediaItem is null)
            throw new KeyNotFoundException("Media item not found.");

        mediaItem.Deactivate();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}