namespace EmotionService.Domain.Entities;

public class MovieDetail
{
    public Guid MediaItemId { get; private set; }
    public MediaItem MediaItem { get; private set; } = default!;
    public string Director { get; private set; } = default!;
    public int? ReleaseYear { get; private set; }
    public int DurationMinutes { get; private set; }
    public string Genre { get; private set; } = default!;
    public string Synopsis { get; private set; } = default!;
    public string? Language { get; private set; }
    public string? Country { get; private set; }
    public string? AgeRating { get; private set; }
    public string? Cast { get; private set; }
    public string? Studio { get; private set; }

    private MovieDetail()
    {
    }

    public static MovieDetail Create(
        Guid mediaItemId,
        string director,
        int? releaseYear,
        int durationMinutes,
        string genre,
        string synopsis,
        string? language,
        string? country,
        string? ageRating,
        string? cast,
        string? studio)
        => new()
        {
            MediaItemId = mediaItemId,
            Director = director.Trim(),
            ReleaseYear = releaseYear,
            DurationMinutes = durationMinutes,
            Genre = genre.Trim(),
            Synopsis = synopsis.Trim(),
            Language = language?.Trim(),
            Country = country?.Trim(),
            AgeRating = ageRating?.Trim(),
            Cast = cast?.Trim(),
            Studio = studio?.Trim()
        };

    public void Update(
        string director,
        int? releaseYear,
        int durationMinutes,
        string genre,
        string synopsis,
        string? language,
        string? country,
        string? ageRating,
        string? cast,
        string? studio)
    {
        Director = director.Trim();
        ReleaseYear = releaseYear;
        DurationMinutes = durationMinutes;
        Genre = genre.Trim();
        Synopsis = synopsis.Trim();
        Language = language?.Trim();
        Country = country?.Trim();
        AgeRating = ageRating?.Trim();
        Cast = cast?.Trim();
        Studio = studio?.Trim();
    }
}
