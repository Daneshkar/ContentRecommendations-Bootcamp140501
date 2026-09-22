using EmotionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmotionService.Application.Features.Recommendations.Get;

internal sealed class RecommendationResultHydrator(
    ApplicationDbContext dbContext)
{
    private const decimal NeutralWeight = 0.05m;

    public async Task<IReadOnlyList<RecommendationItemResponse>> HydrateAsync(
        IReadOnlyCollection<Guid> orderedMediaItemIds,
        string itemTypeName,
        RecommendationNamedValueResponse? primaryMood,
        IReadOnlyCollection<int> matchedMoodIds,
        IReadOnlyCollection<int> matchedThemeIds,
        string recommendationMode,
        CancellationToken cancellationToken)
    {
        var mediaItemIds = orderedMediaItemIds.ToArray();

        var mediaItems = await dbContext.MediaItems
            .AsNoTracking()
            .Where(item =>
                mediaItemIds.Contains(item.Id)
                && item.Status
                && item.ItemType.IsActive)
            .Select(item => new MediaItemProjection(
                item.Id,
                item.Name,
                item.Description,
                item.ImageUrl,
                item.ReleaseDate,
                item.ItemTypeId,
                item.ItemType.Name))
            .ToDictionaryAsync(item => item.Id, cancellationToken);

        var genreRows = await dbContext.MediaItemGenres
            .AsNoTracking()
            .Where(item => mediaItemIds.Contains(item.MediaItemId))
            .Select(item => new MediaItemNamedProjection(
                item.MediaItemId,
                item.GenreId,
                item.Genre.Name))
            .ToListAsync(cancellationToken);

        var moodRows = await dbContext.ItemMoodWeights
            .AsNoTracking()
            .Where(weight =>
                mediaItemIds.Contains(weight.MediaItemId)
                && weight.WeightValue >= NeutralWeight
                && weight.Mood.IsActive)
            .Select(weight => new WeightedNamedProjection(
                weight.MediaItemId,
                weight.MoodId,
                weight.Mood.Name,
                weight.WeightValue,
                weight.ExperienceCount))
            .ToListAsync(cancellationToken);

        var themeRows = await dbContext.ItemThemeWeights
            .AsNoTracking()
            .Where(weight =>
                mediaItemIds.Contains(weight.MediaItemId)
                && weight.WeightValue >= NeutralWeight
                && weight.Theme.IsActive)
            .Select(weight => new WeightedNamedProjection(
                weight.MediaItemId,
                weight.ThemeId,
                weight.Theme.Name,
                weight.WeightValue,
                weight.ExperienceCount))
            .ToListAsync(cancellationToken);

        var details = await LoadDetailsAsync(
            itemTypeName,
            mediaItemIds,
            cancellationToken);

        var genresByItem = genreRows
            .GroupBy(row => row.MediaItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<RecommendationNamedValueResponse>)
                    group
                        .OrderBy(row => row.Name)
                        .ThenBy(row => row.Id)
                        .Select(row =>
                            new RecommendationNamedValueResponse(
                                row.Id,
                                row.Name))
                        .ToArray());

        var topMoodsByItem = CreateTopValues(moodRows);
        var topThemesByItem = CreateTopValues(themeRows);
        var matchedMoodIdSet = matchedMoodIds.ToHashSet();
        var matchedThemeIdSet = matchedThemeIds.ToHashSet();
        var matchedMoodsByItem = CreateMatchedValues(
            moodRows,
            matchedMoodIdSet);
        var matchedThemesByItem = CreateMatchedValues(
            themeRows,
            matchedThemeIdSet);

        var results = new List<RecommendationItemResponse>(
            orderedMediaItemIds.Count);

        foreach (var mediaItemId in orderedMediaItemIds)
        {
            if (!mediaItems.TryGetValue(mediaItemId, out var mediaItem))
            {
                continue;
            }

            var matchedMoods = GetValuesOrEmpty(
                matchedMoodsByItem,
                mediaItem.Id);

            results.Add(new RecommendationItemResponse(
                mediaItem.Id,
                mediaItem.Name,
                mediaItem.Description,
                mediaItem.CoverUrl,
                mediaItem.ReleaseDate,
                new RecommendationNamedValueResponse(
                    mediaItem.ItemTypeId,
                    mediaItem.ItemTypeName),
                GetValuesOrEmpty(genresByItem, mediaItem.Id),
                GetValuesOrEmpty(topMoodsByItem, mediaItem.Id),
                GetValuesOrEmpty(topThemesByItem, mediaItem.Id),
                new RecommendationContextResponse(
                    primaryMood,
                    primaryMood is null ? [] : matchedMoods,
                    GetValuesOrEmpty(matchedThemesByItem, mediaItem.Id),
                    recommendationMode,
                    primaryMood is null ? matchedMoods : null),
                details.GetValueOrDefault(mediaItem.Id)));
        }

        return results;
    }

    private static IReadOnlyDictionary<
        Guid,
        IReadOnlyList<RecommendationNamedValueResponse>> CreateTopValues(
            IEnumerable<WeightedNamedProjection> rows)
        => rows
            .GroupBy(row => row.MediaItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<RecommendationNamedValueResponse>)
                    group
                        .OrderByDescending(row => row.WeightValue)
                        .ThenByDescending(row => row.ExperienceCount)
                        .ThenBy(row => row.Id)
                        .Take(5)
                        .Select(row =>
                            new RecommendationNamedValueResponse(
                                row.Id,
                                row.Name))
                        .ToArray());

    private static IReadOnlyDictionary<
        Guid,
        IReadOnlyList<RecommendationNamedValueResponse>> CreateMatchedValues(
            IEnumerable<WeightedNamedProjection> rows,
            IReadOnlySet<int> selectedIds)
        => rows
            .Where(row => selectedIds.Contains(row.Id))
            .GroupBy(row => row.MediaItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<RecommendationNamedValueResponse>)
                    group
                        .OrderBy(row => row.Id)
                        .Select(row =>
                            new RecommendationNamedValueResponse(
                                row.Id,
                                row.Name))
                        .ToArray());

    private static IReadOnlyList<RecommendationNamedValueResponse>
        GetValuesOrEmpty(
            IReadOnlyDictionary<
                Guid,
                IReadOnlyList<RecommendationNamedValueResponse>> lookup,
            Guid mediaItemId)
        => lookup.TryGetValue(mediaItemId, out var values)
            ? values
            : [];

    private async Task<IReadOnlyDictionary<
        Guid,
        RecommendationDetailsResponse>> LoadDetailsAsync(
            string itemTypeName,
            IReadOnlyCollection<Guid> mediaItemIds,
            CancellationToken cancellationToken)
    {
        if (itemTypeName.Equals("Movie", StringComparison.OrdinalIgnoreCase))
        {
            var rows = await dbContext.MovieDetails
                .AsNoTracking()
                .Where(detail => mediaItemIds.Contains(detail.MediaItemId))
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                detail => detail.MediaItemId,
                detail => (RecommendationDetailsResponse)
                    new MovieRecommendationDetailsResponse(
                        detail.Director,
                        detail.ReleaseYear,
                        detail.DurationMinutes,
                        detail.Genre,
                        detail.Synopsis,
                        detail.Language,
                        detail.Country,
                        detail.AgeRating,
                        detail.Cast,
                        detail.Studio));
        }

        if (itemTypeName.Equals("Music", StringComparison.OrdinalIgnoreCase))
        {
            var rows = await dbContext.MusicDetails
                .AsNoTracking()
                .Where(detail => mediaItemIds.Contains(detail.MediaItemId))
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                detail => detail.MediaItemId,
                detail => (RecommendationDetailsResponse)
                    new MusicRecommendationDetailsResponse(
                        detail.Artist,
                        detail.Album,
                        detail.ReleaseYear,
                        detail.Genre,
                        detail.DurationSeconds,
                        detail.TrackNumber,
                        detail.Description,
                        detail.Publisher,
                        detail.Language));
        }

        if (itemTypeName.Equals("Game", StringComparison.OrdinalIgnoreCase))
        {
            var rows = await dbContext.GameDetails
                .AsNoTracking()
                .Where(detail => mediaItemIds.Contains(detail.MediaItemId))
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                detail => detail.MediaItemId,
                detail => (RecommendationDetailsResponse)
                    new GameRecommendationDetailsResponse(
                        detail.Developer,
                        detail.Publisher,
                        detail.ReleaseYear,
                        detail.Genre,
                        detail.Platform,
                        detail.Description,
                        detail.AgeRating,
                        detail.GameMode,
                        detail.Engine));
        }

        if (itemTypeName.Equals("Book", StringComparison.OrdinalIgnoreCase))
        {
            var rows = await dbContext.BookDetails
                .AsNoTracking()
                .Where(detail => mediaItemIds.Contains(detail.MediaItemId))
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                detail => detail.MediaItemId,
                detail => (RecommendationDetailsResponse)
                    new BookRecommendationDetailsResponse(
                        detail.Author,
                        detail.Publisher,
                        detail.PublicationDate,
                        detail.Genre,
                        detail.ISBN,
                        detail.PageCount,
                        detail.Language,
                        detail.Description,
                        detail.Edition));
        }

        return new Dictionary<Guid, RecommendationDetailsResponse>();
    }

    private sealed record MediaItemProjection(
        Guid Id,
        string Name,
        string? Description,
        string? CoverUrl,
        DateOnly? ReleaseDate,
        int ItemTypeId,
        string ItemTypeName);

    private sealed record MediaItemNamedProjection(
        Guid MediaItemId,
        int Id,
        string Name);

    private sealed record WeightedNamedProjection(
        Guid MediaItemId,
        int Id,
        string Name,
        decimal WeightValue,
        int ExperienceCount);
}
