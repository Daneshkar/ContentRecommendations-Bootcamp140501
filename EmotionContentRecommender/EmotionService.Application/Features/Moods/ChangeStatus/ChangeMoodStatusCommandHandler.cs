using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Moods.ChangeStatus;

public sealed class ChangeMoodStatusCommandHandler
    : IRequestHandler<ChangeMoodStatusCommand, ChangeMoodStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public ChangeMoodStatusCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChangeMoodStatusResponse> Handle(
        ChangeMoodStatusCommand request,
        CancellationToken cancellationToken)
    {
        var mood = await _dbContext.Moods
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (mood is null)
        {
            throw new NotFoundException(
                "احساس مورد نظر یافت نشد");
        }

        if (request.IsActive)
        {
            mood.Activate();
        }
        else
        {
            mood.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangeMoodStatusResponse(
            mood.Id,
            mood.Name,
            mood.IsActive);
    }
}