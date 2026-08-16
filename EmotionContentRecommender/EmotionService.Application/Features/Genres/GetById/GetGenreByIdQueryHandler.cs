using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Genres.GetById;

public sealed class GetGenreByIdQueryHandler
    : IRequestHandler<GetGenreByIdQuery, GetGenreByIdResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetGenreByIdQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetGenreByIdResponse> Handle(
        GetGenreByIdQuery request,
        CancellationToken cancellationToken)
    {
        var genre = await _dbContext.Genres
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetGenreByIdResponse(
                x.Id,
                x.ItemTypeId,
                x.Name,
                x.Description
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (genre is null)
            throw new KeyNotFoundException("Genre not found.");

        return genre;
    }
}