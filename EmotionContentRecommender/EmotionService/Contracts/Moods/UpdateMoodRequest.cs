namespace EmotionService.Contracts.Moods;

public sealed record UpdateMoodRequest(
    string Name,
    string? Description
);