using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Genres.GetAll;

public sealed class GetGenresQueryHandler
    : IRequestHandler<GetGenresQuery, IReadOnlyList<GenreListItemResponse>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetGenresQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GenreListItemResponse>> Handle(
        GetGenresQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Genres
            .AsNoTracking()
            .AsQueryable();

        if (request.ItemTypeId.HasValue)
        {
            query = query.Where(
                x => x.ItemTypeId == request.ItemTypeId.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new GenreListItemResponse(
                x.Id,
                x.ItemTypeId,
                x.Name,
                x.Description
            ))
            .ToListAsync(cancellationToken);
    }
}