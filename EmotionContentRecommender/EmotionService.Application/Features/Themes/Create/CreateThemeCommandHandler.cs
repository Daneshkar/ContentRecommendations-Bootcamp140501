using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Themes.Create;

public sealed class CreateThemeCommandHandler
    : IRequestHandler<CreateThemeCommand, CreateThemeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateThemeCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateThemeResponse> Handle(
        CreateThemeCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var duplicateExists = await _dbContext.Themes
            .AnyAsync(
                x => x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                "مضمونی با این نام از قبل وجود دارد");
        }

        var theme = Theme.Create(
            normalizedName,
            request.Description);

        _dbContext.Themes.Add(theme);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateThemeResponse(
            theme.Id,
            theme.Name,
            theme.Description,
            theme.IsActive);
    }
}