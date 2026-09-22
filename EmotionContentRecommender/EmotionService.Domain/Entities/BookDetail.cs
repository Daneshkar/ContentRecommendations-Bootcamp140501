namespace EmotionService.Domain.Entities;

public class BookDetail
{
    public Guid MediaItemId { get; private set; }
    public MediaItem MediaItem { get; private set; } = default!;
    public string Author { get; private set; } = default!;
    public string Publisher { get; private set; } = default!;
    public DateOnly? PublicationDate { get; private set; }
    public string Genre { get; private set; } = default!;
    public string ISBN { get; private set; } = default!;
    public int? PageCount { get; private set; }
    public string Language { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string? Edition { get; private set; }

    private BookDetail()
    {
    }

    public static BookDetail Create(
        Guid mediaItemId,
        string author,
        string publisher,
        DateOnly? publicationDate,
        string genre,
        string isbn,
        int? pageCount,
        string language,
        string description,
        string? edition)
        => new()
        {
            MediaItemId = mediaItemId,
            Author = author.Trim(),
            Publisher = publisher.Trim(),
            PublicationDate = publicationDate,
            Genre = genre.Trim(),
            ISBN = isbn.Trim(),
            PageCount = pageCount,
            Language = language.Trim(),
            Description = description.Trim(),
            Edition = edition?.Trim()
        };

    public void Update(
        string author,
        string publisher,
        DateOnly? publicationDate,
        string genre,
        string isbn,
        int? pageCount,
        string language,
        string description,
        string? edition)
    {
        Author = author.Trim();
        Publisher = publisher.Trim();
        PublicationDate = publicationDate;
        Genre = genre.Trim();
        ISBN = isbn.Trim();
        PageCount = pageCount;
        Language = language.Trim();
        Description = description.Trim();
        Edition = edition?.Trim();
    }
}
