using EmotionService.Application.Features.Experiences.Common;
using MediatR;

namespace EmotionService.Application.Features.Experiences.GetMine;

public sealed record GetMyExperiencesQuery(long UserId)
    : IRequest<IReadOnlyList<ExperienceResponse>>;
