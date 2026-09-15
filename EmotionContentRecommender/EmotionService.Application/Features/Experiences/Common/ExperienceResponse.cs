namespace EmotionService.Application.Features.Experiences.Common;

public sealed record ExperienceResponse(
    Guid Id,
    Guid MediaItemId,
    string MediaItemName,
    string? MediaItemImageUrl,
    int Score,
    string? Note,
    IReadOnlyCollection<ExperienceMoodResponse> Moods,
    IReadOnlyCollection<ExperienceThemeResponse> Themes,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record ExperienceMoodResponse(
    int Id,
    string Name
);

public sealed record ExperienceThemeResponse(
    int Id,
    string Name
);
