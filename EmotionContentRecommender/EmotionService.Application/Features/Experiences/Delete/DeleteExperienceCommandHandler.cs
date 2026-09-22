using EmotionService.Infrastructure.BackgroundJobs;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Experiences.Delete;

public sealed class DeleteExperienceCommandHandler(
    ApplicationDbContext dbContext,
    AggregateWeightQueue aggregateWeightQueue)
    : IRequestHandler<DeleteExperienceCommand>
{
    public async Task Handle(
        DeleteExperienceCommand command,
        CancellationToken cancellationToken)
    {
        var experience = await dbContext.Experiences
            .FirstOrDefaultAsync(
                x => x.Id == command.Id && x.UserId == command.UserId,
                cancellationToken);

        if (experience is null)
        {
            throw new NotFoundException("تجربه‌ی مورد نظر یافت نشد.");
        }

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        dbContext.Experiences.Remove(experience);
        await dbContext.SaveChangesAsync(cancellationToken);

        await aggregateWeightQueue.MarkPendingAsync(
            experience.MediaItemId,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
