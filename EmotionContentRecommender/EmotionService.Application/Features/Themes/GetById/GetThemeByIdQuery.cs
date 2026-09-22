using MediatR;

namespace EmotionService.Application.Features.Themes.GetById;

public sealed record GetThemeByIdQuery(
    int Id
) : IRequest<GetThemeByIdResponse>;