using MediatR;

namespace EmotionService.Application.Features.MediaItems.GetById;

public sealed record GetMediaItemByIdQuery(Guid Id)
    : IRequest<GetMediaItemByIdResponse>;