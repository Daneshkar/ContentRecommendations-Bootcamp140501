using MediatR;

namespace EmotionService.Application.Features.Moods.Create;

public sealed record CreateMoodCommand(
    string Name,
    string? Description
) : IRequest<CreateMoodResponse>;