using MediatR;

namespace EmotionService.Application.Features.Genres.GetById;

public sealed record GetGenreByIdQuery(int Id)
    : IRequest<GetGenreByIdResponse>;