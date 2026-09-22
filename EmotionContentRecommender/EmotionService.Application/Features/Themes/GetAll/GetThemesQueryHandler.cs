using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Themes.GetAll;

public sealed class GetThemesQueryHandler
    : IRequestHandler<GetThemesQuery, IReadOnlyList<ThemeListItemResponse>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetThemesQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ThemeListItemResponse>> Handle(
        GetThemesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Themes
            .AsNoTracking()
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive == request.IsActive.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new ThemeListItemResponse(
                x.Id,
                x.Name,
                x.Description,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}