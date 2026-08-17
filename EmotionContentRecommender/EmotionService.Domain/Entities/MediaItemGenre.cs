namespace EmotionService.Domain.Entities;

public class MediaItemGenre
{
    public Guid MediaItemId { get; private set; }

    public int GenreId { get; private set; }

    public MediaItem MediaItem { get; private set; } = default!;

    public Genre Genre { get; private set; } = default!;

    private MediaItemGenre()
    {
    }

    public static MediaItemGenre Create(
        Guid mediaItemId,
        int genreId)
    {
        return new MediaItemGenre
        {
            MediaItemId = mediaItemId,
            GenreId = genreId
        };
    }
}