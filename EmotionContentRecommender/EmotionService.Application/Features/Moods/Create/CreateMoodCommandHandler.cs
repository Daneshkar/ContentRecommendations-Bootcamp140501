using EmotionService.Domain.Entities;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Moods.Create;

public sealed class CreateMoodCommandHandler
    : IRequestHandler<CreateMoodCommand, CreateMoodResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateMoodCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateMoodResponse> Handle(
        CreateMoodCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var duplicateExists = await _dbContext.Moods
            .AnyAsync(
                x => x.Name == normalizedName,
                cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                "احساساتی با این نام از قبل وجود دارد");
        }

        var mood = Mood.Create(
            normalizedName,
            request.Description);

        _dbContext.Moods.Add(mood);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateMoodResponse(
            mood.Id,
            mood.Name,
            mood.Description,
            mood.IsActive);
    }
}