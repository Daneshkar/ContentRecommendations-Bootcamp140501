namespace EmotionService.Contracts.MediaDetails;

public sealed record MovieDetailRequest(
    string Director,
    int? ReleaseYear,
    int DurationMinutes,
    string Genre,
    string Synopsis,
    string? Language,
    string? Country,
    string? AgeRating,
    string? Cast,
    string? Studio);
