using System.Text.Json.Serialization;

namespace EmotionService.Application.Features.Recommendations.Get;

public sealed record GetRecommendationsResponse(
    RecommendationCriteriaResponse Criteria,
    IReadOnlyList<RecommendationItemResponse> Items,
    string? NextCursor,
    bool HasMore,
    string? EmptyReason);

public sealed record RecommendationCriteriaResponse(
    RecommendationNamedValueResponse ItemType,
    RecommendationNamedValueResponse PrimaryMood,
    IReadOnlyList<RecommendationNamedValueResponse> AdditionalMoods,
    IReadOnlyList<RecommendationNamedValueResponse> Themes);

public sealed record RecommendationItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string? CoverUrl,
    DateOnly? ReleaseDate,
    RecommendationNamedValueResponse ItemType,
    IReadOnlyList<RecommendationNamedValueResponse> Genres,
    IReadOnlyList<RecommendationNamedValueResponse> TopMoods,
    IReadOnlyList<RecommendationNamedValueResponse> TopThemes,
    RecommendationContextResponse RecommendationContext,
    RecommendationDetailsResponse? Details);

public sealed record RecommendationContextResponse(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    RecommendationNamedValueResponse? PrimaryMood,
    IReadOnlyList<RecommendationNamedValueResponse> MatchedAdditionalMoods,
    IReadOnlyList<RecommendationNamedValueResponse> MatchedThemes,
    string Mode,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<RecommendationNamedValueResponse>? MatchedMoods = null);

public sealed record RecommendationNamedValueResponse(
    int Id,
    string Name);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(MovieRecommendationDetailsResponse), "movie")]
[JsonDerivedType(typeof(MusicRecommendationDetailsResponse), "music")]
[JsonDerivedType(typeof(GameRecommendationDetailsResponse), "game")]
[JsonDerivedType(typeof(BookRecommendationDetailsResponse), "book")]
public abstract record RecommendationDetailsResponse;

public sealed record MovieRecommendationDetailsResponse(
    string Director,
    int? ReleaseYear,
    int DurationMinutes,
    string Genre,
    string Synopsis,
    string? Language,
    string? Country,
    string? AgeRating,
    string? Cast,
    string? Studio)
    : RecommendationDetailsResponse;

public sealed record MusicRecommendationDetailsResponse(
    string Artist,
    string? Album,
    int? ReleaseYear,
    string Genre,
    int DurationSeconds,
    int? TrackNumber,
    string? Description,
    string? Publisher,
    string? Language)
    : RecommendationDetailsResponse;

public sealed record GameRecommendationDetailsResponse(
    string Developer,
    string Publisher,
    int? ReleaseYear,
    string Genre,
    string Platform,
    string Description,
    string? AgeRating,
    string? GameMode,
    string? Engine)
    : RecommendationDetailsResponse;

public sealed record BookRecommendationDetailsResponse(
    string Author,
    string Publisher,
    DateOnly? PublicationDate,
    string Genre,
    string Isbn,
    int? PageCount,
    string Language,
    string Description,
    string? Edition)
    : RecommendationDetailsResponse;
