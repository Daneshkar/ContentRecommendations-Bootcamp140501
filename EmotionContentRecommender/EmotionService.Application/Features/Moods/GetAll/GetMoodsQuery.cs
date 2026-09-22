using MediatR;

namespace EmotionService.Application.Features.Moods.GetAll;

public sealed record GetMoodsQuery(
    bool? IsActive
) : IRequest<IReadOnlyList<MoodListItemResponse>>;