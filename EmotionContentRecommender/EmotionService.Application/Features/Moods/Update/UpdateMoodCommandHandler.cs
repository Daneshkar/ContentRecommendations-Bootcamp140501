using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Moods.Update;

public sealed class UpdateMoodCommandHandler
    : IRequestHandler<UpdateMoodCommand, UpdateMoodResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateMoodCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateMoodResponse> Handle(
        UpdateMoodCommand request,
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

        var normalizedName = request.Name.Trim();

        var duplicateExists = await _dbContext.Moods
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                "احساسی با این نام از قبل وجود دارد");
        }

        mood.Update(
            normalizedName,
            request.Description);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateMoodResponse(
            mood.Id,
            mood.Name,
            mood.Description,
            mood.IsActive);
    }
}