using System.Text.Json.Serialization;

namespace EmotionClient.Models;

public static class RecommendationCatalog
{
    public static readonly IReadOnlyList<RecommendationCategory> Categories =
    [
        new(1, "Movie", "Movies", "Stories to watch", "movie"),
        new(3, "Book", "Books", "Worlds to read", "book"),
        new(2, "Game", "Games", "Adventures to play", "game"),
        new(4, "Music", "Music", "Sounds to feel", "music")
    ];

    public static bool IsKnownItemType(int itemTypeId)
        => Categories.Any(category => category.Id == itemTypeId);
}

public sealed record RecommendationCategory(
    int Id,
    string Name,
    string PluralName,
    string Description,
    string CssClass);

public sealed class MoodRecommendationSelectionModel
{
    public int ItemTypeId { get; set; }
    public int PrimaryMoodId { get; set; }
    public List<int> AdditionalMoodIds { get; set; } = [];
    public List<int> ThemeIds { get; set; } = [];
    public string? Cursor { get; set; }
}

public sealed class MoodRecommendationPageViewModel
{
    public MoodRecommendationSelectionModel Selection { get; init; } = new();
    public IReadOnlyList<RecommendationLookupOption> Moods { get; init; } = [];
    public IReadOnlyList<RecommendationLookupOption> Themes { get; init; } = [];
    public string? LookupError { get; init; }
    public string? SubmissionError { get; init; }
}

public sealed class RecommendationResultViewModel
{
    public MoodRecommendationSelectionModel Selection { get; init; } = new();
    public ExperienceRecommendationSelectionModel? ExperienceSelection { get; init; }
    public RecommendationResponse? Response { get; init; }
    public ExperienceRecommendationResponse? ExperienceResponse { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsExperience => ExperienceSelection is not null;
    public RecommendationItem? Item
        => Response?.Items.FirstOrDefault()
            ?? ExperienceResponse?.Items.FirstOrDefault();
    public string? NextCursor
        => Response?.NextCursor ?? ExperienceResponse?.NextCursor;
    public bool HasMore
        => Response?.HasMore ?? ExperienceResponse?.HasMore ?? false;
    public string? EmptyReason
        => Response?.EmptyReason ?? ExperienceResponse?.EmptyReason;
}

public sealed class ExperienceStartSelectionModel
{
    public Guid MediaItemId { get; set; }
}

public sealed class ExperienceStartPageViewModel
{
    public ExperienceStartSelectionModel Selection { get; init; } = new();
    public IReadOnlyList<MediaItemOption> MediaItems { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public sealed class ExperienceEntryModel
{
    public Guid MediaItemId { get; set; }
    public int Score { get; set; }
    public List<int> MoodIds { get; set; } = [];
    public List<int> ThemeIds { get; set; } = [];
}

public sealed class ExperienceEntryPageViewModel
{
    public ExperienceEntryModel Entry { get; init; } = new();
    public MediaItemOption MediaItem { get; init; } = new();
    public IReadOnlyList<RecommendationLookupOption> Moods { get; init; } = [];
    public IReadOnlyList<RecommendationLookupOption> Themes { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public sealed class ExperienceThanksViewModel
{
    public string MediaItemName { get; init; } = string.Empty;
    public int Score { get; init; }
}

public sealed class ExperienceRecommendationSelectionModel
{
    public Guid ExperienceId { get; set; }
    public string? Cursor { get; set; }
}

public sealed class MediaItemOption
{
    public Guid Id { get; set; }
    public int ItemTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? CoverUrl { get; set; }
    public bool IsActive { get; set; }

    public RecommendationCategory? Category
        => RecommendationCatalog.Categories.FirstOrDefault(item => item.Id == ItemTypeId);
}

public sealed class UserExperienceSummary
{
    public Guid Id { get; set; }
    public Guid MediaItemId { get; set; }
    public string MediaItemName { get; set; } = string.Empty;
    public string? MediaItemImageUrl { get; set; }
    public int Score { get; set; }
}

public sealed class CreateExperienceResponse
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public Guid MediaItemId { get; set; }
    public int Score { get; set; }
    public List<int> MoodIds { get; set; } = [];
    public List<int> ThemeIds { get; set; } = [];
}

public sealed class RecommendationLookupOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public sealed class RecommendationResponse
{
    public RecommendationCriteria Criteria { get; set; } = new();
    public List<RecommendationItem> Items { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public string? EmptyReason { get; set; }
}

public sealed class ExperienceRecommendationResponse
{
    public ExperienceRecommendationCriteria Criteria { get; set; } = new();
    public List<RecommendationItem> Items { get; set; } = [];
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public string? EmptyReason { get; set; }
}

public sealed class ExperienceRecommendationCriteria
{
    public Guid ExperienceId { get; set; }
    public ExperienceSourceMediaItem SourceMediaItem { get; set; } = new();
    public RecommendationNamedValue ItemType { get; set; } = new();
    public List<RecommendationNamedValue> Moods { get; set; } = [];
    public List<RecommendationNamedValue> Themes { get; set; } = [];
}

public sealed class ExperienceSourceMediaItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RecommendationCriteria
{
    public RecommendationNamedValue ItemType { get; set; } = new();
    public RecommendationNamedValue PrimaryMood { get; set; } = new();
    public List<RecommendationNamedValue> AdditionalMoods { get; set; } = [];
    public List<RecommendationNamedValue> Themes { get; set; } = [];
}

public sealed class RecommendationItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public RecommendationNamedValue ItemType { get; set; } = new();
    public List<RecommendationNamedValue> Genres { get; set; } = [];
    public List<RecommendationNamedValue> TopMoods { get; set; } = [];
    public List<RecommendationNamedValue> TopThemes { get; set; } = [];
    public RecommendationContext RecommendationContext { get; set; } = new();
    public RecommendationDetails? Details { get; set; }
}

public sealed class RecommendationNamedValue
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RecommendationContext
{
    public RecommendationNamedValue? PrimaryMood { get; set; }
    public List<RecommendationNamedValue> MatchedAdditionalMoods { get; set; } = [];
    public List<RecommendationNamedValue> MatchedThemes { get; set; } = [];
    public List<RecommendationNamedValue>? MatchedMoods { get; set; }
    public string Mode { get; set; } = string.Empty;
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(MovieRecommendationDetails), "movie")]
[JsonDerivedType(typeof(MusicRecommendationDetails), "music")]
[JsonDerivedType(typeof(GameRecommendationDetails), "game")]
[JsonDerivedType(typeof(BookRecommendationDetails), "book")]
public abstract class RecommendationDetails;

public sealed class MovieRecommendationDetails : RecommendationDetails
{
    public string Director { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public int DurationMinutes { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Synopsis { get; set; } = string.Empty;
    public string? Language { get; set; }
    public string? Country { get; set; }
    public string? AgeRating { get; set; }
    public string? Cast { get; set; }
    public string? Studio { get; set; }
}

public sealed class MusicRecommendationDetails : RecommendationDetails
{
    public string Artist { get; set; } = string.Empty;
    public string? Album { get; set; }
    public int? ReleaseYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int? TrackNumber { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public string? Language { get; set; }
}

public sealed class GameRecommendationDetails : RecommendationDetails
{
    public string Developer { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AgeRating { get; set; }
    public string? GameMode { get; set; }
    public string? Engine { get; set; }
}

public sealed class BookRecommendationDetails : RecommendationDetails
{
    public string Author { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateOnly? PublicationDate { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int? PageCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Edition { get; set; }
}

public sealed record EmotionApiResult<T>(
    bool IsSuccess,
    T? Data,
    int StatusCode,
    string? ErrorMessage)
{
    public static EmotionApiResult<T> Success(T data, int statusCode = 200)
        => new(true, data, statusCode, null);

    public static EmotionApiResult<T> Failure(int statusCode, string message)
        => new(false, default, statusCode, message);
}
