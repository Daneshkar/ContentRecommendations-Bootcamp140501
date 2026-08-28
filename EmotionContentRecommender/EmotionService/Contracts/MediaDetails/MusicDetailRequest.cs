namespace EmotionService.Contracts.MediaDetails;

public sealed record MusicDetailRequest(
    string Artist,
    string? Album,
    int? ReleaseYear,
    string Genre,
    int DurationSeconds,
    int? TrackNumber,
    string? Description,
    string? Publisher,
    string? Language);
