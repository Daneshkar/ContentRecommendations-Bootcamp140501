using EmotionService.Application.Features.Experiences.Common;
using MediatR;

namespace EmotionService.Application.Features.Experiences.GetById;

public sealed record GetExperienceByIdQuery(
    Guid Id,
    long UserId
) : IRequest<ExperienceResponse>;
