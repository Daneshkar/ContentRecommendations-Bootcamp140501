namespace EmotionService.Domain.Entities;

public class MusicDetail
{
    public Guid MediaItemId { get; private set; }
    public MediaItem MediaItem { get; private set; } = default!;
    public string Artist { get; private set; } = default!;
    public string? Album { get; private set; }
    public int? ReleaseYear { get; private set; }
    public string Genre { get; private set; } = default!;
    public int DurationSeconds { get; private set; }
    public int? TrackNumber { get; private set; }
    public string? Description { get; private set; }
    public string? Publisher { get; private set; }
    public string? Language { get; private set; }

    private MusicDetail()
    {
    }

    public static MusicDetail Create(
        Guid mediaItemId,
        string artist,
        string? album,
        int? releaseYear,
        string genre,
        int durationSeconds,
        int? trackNumber,
        string? description,
        string? publisher,
        string? language)
        => new()
        {
            MediaItemId = mediaItemId,
            Artist = artist.Trim(),
            Album = album?.Trim(),
            ReleaseYear = releaseYear,
            Genre = genre.Trim(),
            DurationSeconds = durationSeconds,
            TrackNumber = trackNumber,
            Description = description?.Trim(),
            Publisher = publisher?.Trim(),
            Language = language?.Trim()
        };

    public void Update(
        string artist,
        string? album,
        int? releaseYear,
        string genre,
        int durationSeconds,
        int? trackNumber,
        string? description,
        string? publisher,
        string? language)
    {
        Artist = artist.Trim();
        Album = album?.Trim();
        ReleaseYear = releaseYear;
        Genre = genre.Trim();
        DurationSeconds = durationSeconds;
        TrackNumber = trackNumber;
        Description = description?.Trim();
        Publisher = publisher?.Trim();
        Language = language?.Trim();
    }
}
