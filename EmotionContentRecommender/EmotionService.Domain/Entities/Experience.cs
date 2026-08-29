namespace EmotionService.Domain.Entities;

public sealed class Experience
{
    public Guid Id { get; private set; }

    public int UserId { get; private set; }

    public Guid MediaItemId { get; private set; }

    public MediaItem MediaItem { get; private set; } = default!;

    public string? Note { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Experience()
    {
    }

    private Experience(
        int userId,
        Guid mediaItemId,
        string? note)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        MediaItemId = mediaItemId;
        Note = NormalizeNote(note);
        CreatedAt = DateTime.UtcNow;
    }

    public static Experience Create(
        int userId,
        Guid mediaItemId,
        string? note = null)
    {
        if (userId <= 0)
        {
            throw new ArgumentException(
                "User is required.",
                nameof(userId));
        }

        if (mediaItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "مدیا ایتم مورد نیاز است",
                nameof(mediaItemId));
        }

        return new Experience(
            userId,
            mediaItemId,
            note);
    }

    public void UpdateNote(string? note)
    {
        Note = NormalizeNote(note);
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeNote(string? note)
    {
        return string.IsNullOrWhiteSpace(note)
            ? null
            : note.Trim();
    }
}