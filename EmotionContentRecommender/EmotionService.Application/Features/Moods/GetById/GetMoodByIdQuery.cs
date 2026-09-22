using MediatR;

namespace EmotionService.Application.Features.Moods.GetById;

public sealed record GetMoodByIdQuery(
    int Id
) : IRequest<GetMoodByIdResponse>;