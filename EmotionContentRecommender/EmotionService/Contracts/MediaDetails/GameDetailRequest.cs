namespace EmotionService.Contracts.MediaDetails;

public sealed record GameDetailRequest(
    string Developer,
    string Publisher,
    int? ReleaseYear,
    string Genre,
    string Platform,
    string Description,
    string? AgeRating,
    string? GameMode,
    string? Engine);
