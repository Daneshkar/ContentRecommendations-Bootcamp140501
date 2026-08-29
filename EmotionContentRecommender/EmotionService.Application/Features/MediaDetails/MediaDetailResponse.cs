using EmotionService.Domain.Entities;

namespace EmotionService.Application.Features.MediaDetails;

public sealed class MediaDetailResponse
{
    public Guid MediaItemId { get; }
    public object? Details { get; }

    public MediaDetailResponse(Guid mediaItemId)
    {
        MediaItemId = mediaItemId;
    }
    public MediaDetailResponse(MusicDetail detail)
    {
        MediaItemId = detail.MediaItemId;
        Details = new MusicDetailDto(
            detail.MediaItemId,
            detail.Artist,
            detail.Album,
            detail.ReleaseYear,
            detail.Genre,
            detail.DurationSeconds,
            detail.TrackNumber,
            detail.Description,
            detail.Publisher,
            detail.Language);
    }

    public MediaDetailResponse(MovieDetail detail)
    {
        MediaItemId = detail.MediaItemId;
        Details = new MovieDetailDto(
            detail.MediaItemId,
            detail.Director,
            detail.ReleaseYear,
            detail.DurationMinutes,
            detail.Genre,
            detail.Synopsis,
            detail.Language,
            detail.Country,
            detail.AgeRating,
            detail.Cast,
            detail.Studio);
    }

    public MediaDetailResponse(GameDetail detail)
    {
        MediaItemId = detail.MediaItemId;
        Details = new GameDetailDto(
            detail.MediaItemId,
            detail.Developer,
            detail.Publisher,
            detail.ReleaseYear,
            detail.Genre,
            detail.Platform,
            detail.Description,
            detail.AgeRating,
            detail.GameMode,
            detail.Engine);
    }

    public MediaDetailResponse(BookDetail detail)
    {
        MediaItemId = detail.MediaItemId;
        Details = new BookDetailDto(
            detail.MediaItemId,
            detail.Author,
            detail.Publisher,
            detail.PublicationDate,
            detail.Genre,
            detail.ISBN,
            detail.PageCount,
            detail.Language,
            detail.Description,
            detail.Edition);
    }
}

public sealed record MusicDetailDto(
    Guid MediaItemId,
    string Artist,
    string? Album,
    int? ReleaseYear,
    string Genre,
    int DurationSeconds,
    int? TrackNumber,
    string? Description,
    string? Publisher,
    string? Language);

public sealed record MovieDetailDto(
    Guid MediaItemId,
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

public sealed record GameDetailDto(
    Guid MediaItemId,
    string Developer,
    string Publisher,
    int? ReleaseYear,
    string Genre,
    string Platform,
    string Description,
    string? AgeRating,
    string? GameMode,
    string? Engine);

public sealed record BookDetailDto(
    Guid MediaItemId,
    string Author,
    string Publisher,
    DateOnly? PublicationDate,
    string Genre,
    string ISBN,
    int? PageCount,
    string Language,
    string Description,
    string? Edition);
