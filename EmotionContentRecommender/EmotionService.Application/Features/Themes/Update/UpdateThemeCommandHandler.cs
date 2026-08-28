using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Themes.Update;

public sealed class UpdateThemeCommandHandler
    : IRequestHandler<UpdateThemeCommand, UpdateThemeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateThemeCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateThemeResponse> Handle(
        UpdateThemeCommand request,
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

        var normalizedName = request.Name.Trim();

        var duplicateExists = await _dbContext.Themes
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                "مضمونی با این نام از قبل وجود دارد");
        }

        theme.Update(
            normalizedName,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateThemeResponse(
            theme.Id,
            theme.Name,
            theme.Description,
            theme.IsActive);
    }
}