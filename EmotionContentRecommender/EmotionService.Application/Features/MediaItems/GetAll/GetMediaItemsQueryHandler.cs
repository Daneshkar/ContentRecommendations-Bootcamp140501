using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.MediaItems.GetAll;

public sealed class GetMediaItemsQueryHandler
    : IRequestHandler<
        GetMediaItemsQuery,
        IReadOnlyList<MediaItemListItemResponse>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMediaItemsQueryHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MediaItemListItemResponse>> Handle(
        GetMediaItemsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.MediaItems
            .AsNoTracking()
            .Where(x => x.Status)
            .AsQueryable();

        if (request.ItemTypeId.HasValue)
        {
            query = query.Where(
                x => x.ItemTypeId == request.ItemTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(
                x => x.Name.Contains(search));
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new MediaItemListItemResponse(
                x.Id,
                x.ItemTypeId,
                x.Name,
                x.Description,
                x.ReleaseDate,
                x.ImageUrl,
                x.Status
            ))
            .ToListAsync(cancellationToken);
    }
}