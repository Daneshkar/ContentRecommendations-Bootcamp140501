using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Moods.GetById;

public sealed class GetMoodByIdQueryHandler
    : IRequestHandler<GetMoodByIdQuery, GetMoodByIdResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMoodByIdQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetMoodByIdResponse> Handle(
        GetMoodByIdQuery request,
        CancellationToken cancellationToken)
    {
        var mood = await _dbContext.Moods
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetMoodByIdResponse(
                x.Id,
                x.Name,
                x.Description,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (mood is null)
        {
            throw new NotFoundException(
                "احساس مورد نظر یافت نشد");
        }

        return mood;
    }
}