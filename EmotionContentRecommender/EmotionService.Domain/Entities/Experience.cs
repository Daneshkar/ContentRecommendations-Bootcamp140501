namespace EmotionService.Domain.Entities;

public sealed class Experience
{
    public Guid Id { get; private set; }

    public long UserId { get; private set; }

    public int Score { get; private set; }

    public Guid MediaItemId { get; private set; }

    public MediaItem MediaItem { get; private set; } = default!;

    public string? Note { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ExperienceMood> _experienceMoods = [];

    private readonly List<ExperienceTheme> _experienceThemes = [];

    public IReadOnlyCollection<ExperienceMood> ExperienceMoods
        => _experienceMoods;

    public IReadOnlyCollection<ExperienceTheme> ExperienceThemes
        => _experienceThemes;

    private Experience()
    {
    }

    private Experience(
        long userId,
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
        long userId,
        int score,
        Guid mediaItemId,
        string? note = null)
    {
        if (userId <= 0)
        {
            throw new ArgumentException(
                "User is required.",
                nameof(userId));
        }

        if (score is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "Score must be between 1 and 5.");

        if (mediaItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "Media Item needed",
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

    public void UpdateScore(int score)
    {
        if (score is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(score),
                "Score must be between 1 and 5.");

        Score = score;
        UpdatedAt = DateTime.UtcNow;
    }
}