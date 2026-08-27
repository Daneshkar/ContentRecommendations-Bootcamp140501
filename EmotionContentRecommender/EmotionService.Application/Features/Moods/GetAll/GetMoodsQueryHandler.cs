using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Moods.GetAll;

public sealed class GetMoodsQueryHandler
    : IRequestHandler<GetMoodsQuery, IReadOnlyList<MoodListItemResponse>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMoodsQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MoodListItemResponse>> Handle(
        GetMoodsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Moods
            .AsNoTracking()
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive == request.IsActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new MoodListItemResponse(
                x.Id,
                x.Name,
                x.Description,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}