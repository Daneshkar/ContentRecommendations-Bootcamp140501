using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Experiences.Delete;

public sealed class DeleteExperienceCommandHandler(
    ApplicationDbContext dbContext)
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

        dbContext.Experiences.Remove(experience);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
