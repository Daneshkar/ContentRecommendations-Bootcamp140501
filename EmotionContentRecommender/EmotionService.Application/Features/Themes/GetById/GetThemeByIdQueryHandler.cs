using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Themes.GetById;

public sealed class GetThemeByIdQueryHandler
    : IRequestHandler<GetThemeByIdQuery, GetThemeByIdResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetThemeByIdQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetThemeByIdResponse> Handle(
        GetThemeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var theme = await _dbContext.Themes
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetThemeByIdResponse(
                x.Id,
                x.Name,
                x.Description,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (theme is null)
        {
            throw new NotFoundException(
                "مضمون مورد نظر یافت نشد");
        }

        return theme;
    }
}