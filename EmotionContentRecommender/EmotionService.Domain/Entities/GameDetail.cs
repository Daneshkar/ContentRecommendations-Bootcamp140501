namespace EmotionService.Domain.Entities;

public class GameDetail
{
    public Guid MediaItemId { get; private set; }
    public MediaItem MediaItem { get; private set; } = default!;
    public string Developer { get; private set; } = default!;
    public string Publisher { get; private set; } = default!;
    public int? ReleaseYear { get; private set; }
    public string Genre { get; private set; } = default!;
    public string Platform { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string? AgeRating { get; private set; }
    public string? GameMode { get; private set; }
    public string? Engine { get; private set; }

    private GameDetail()
    {
    }

    public static GameDetail Create(
        Guid mediaItemId,
        string developer,
        string publisher,
        int? releaseYear,
        string genre,
        string platform,
        string description,
        string? ageRating,
        string? gameMode,
        string? engine)
        => new()
        {
            MediaItemId = mediaItemId,
            Developer = developer.Trim(),
            Publisher = publisher.Trim(),
            ReleaseYear = releaseYear,
            Genre = genre.Trim(),
            Platform = platform.Trim(),
            Description = description.Trim(),
            AgeRating = ageRating?.Trim(),
            GameMode = gameMode?.Trim(),
            Engine = engine?.Trim()
        };

    public void Update(
        string developer,
        string publisher,
        int? releaseYear,
        string genre,
        string platform,
        string description,
        string? ageRating,
        string? gameMode,
        string? engine)
    {
        Developer = developer.Trim();
        Publisher = publisher.Trim();
        ReleaseYear = releaseYear;
        Genre = genre.Trim();
        Platform = platform.Trim();
        Description = description.Trim();
        AgeRating = ageRating?.Trim();
        GameMode = gameMode?.Trim();
        Engine = engine?.Trim();
    }
}
