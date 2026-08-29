using MediatR;

namespace EmotionService.Application.Features.Moods.Update;

public sealed record UpdateMoodCommand(
    int Id,
    string Name,
    string? Description
) : IRequest<UpdateMoodResponse>;