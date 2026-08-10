
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EmotionService.Infrastructure.Exceptions;


namespace EmotionService.Application.Features.MediaItems.GetById;

public sealed class GetMediaItemByIdQueryHandler
    : IRequestHandler<GetMediaItemByIdQuery, GetMediaItemByIdResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMediaItemByIdQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetMediaItemByIdResponse> Handle(
        GetMediaItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var mediaItem = await _dbContext.MediaItems
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.Status)
            .Select(x => new GetMediaItemByIdResponse(
                x.Id,
                x.ItemTypeId,
                x.Name,
                x.Description,
                x.ReleaseDate,
                x.ImageUrl,
                x.Status,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (mediaItem is null)
            throw new NotFoundException("Media item not found.");

        return mediaItem;
    }
}