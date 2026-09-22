using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Themes.ChangeStatus;

public sealed class ChangeThemeStatusCommandHandler
    : IRequestHandler<ChangeThemeStatusCommand, ChangeThemeStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public ChangeThemeStatusCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangeThemeStatusResponse> Handle(
        ChangeThemeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var theme = await _dbContext.Themes
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (theme is null)
        {
            throw new NotFoundException(
                "مضمون مورد نظر یافت نشد");
        }

        if (request.IsActive)
        {
            theme.Activate();
        }
        else
        {
            theme.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeThemeStatusResponse(
            theme.Id,
            theme.Name,
            theme.IsActive);
    }
}