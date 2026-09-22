namespace EmotionService.Application.Features.Moods.GetById;

public sealed record GetMoodByIdResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive
);