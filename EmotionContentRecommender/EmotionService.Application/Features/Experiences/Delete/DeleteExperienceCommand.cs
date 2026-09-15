using MediatR;

namespace EmotionService.Application.Features.Experiences.Delete;

public sealed record DeleteExperienceCommand(
    Guid Id,
    long UserId
) : IRequest;
