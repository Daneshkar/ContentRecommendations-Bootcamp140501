namespace EmotionService.Contracts.Moods;

public sealed record CreateMoodRequest(
    string Name,
    string? Description
);