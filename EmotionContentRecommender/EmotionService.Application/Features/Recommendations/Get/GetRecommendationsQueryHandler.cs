using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Jwt;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EmotionService.Application.Features.Recommendations.Get;

public sealed class GetRecommendationsQueryHandler(
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<GetRecommendationsQuery, GetRecommendationsResponse>
{
    private const decimal NeutralWeight = 0.05m;
    private const decimal MaximumWeight = 0.10m;
    private const decimal PrimaryMoodCoefficient = 0.70m;
    private const decimal AdditionalMoodCoefficient = 0.20m;
    private const decimal ThemeCoefficient = 0.10m;
    private const string RecommendationMode = "MoodFirst";

    public async Task<GetRecommendationsResponse> Handle(
        GetRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var itemType = await GetActiveItemTypeAsync(
            request.ItemTypeId,
            cancellationToken);

        var moodLookup = await GetActiveMoodsAsync(
            request,
            cancellationToken);

        var themeLookup = await GetActiveThemesAsync(
            request.ThemeIds,
            cancellationToken);

        var criteria = CreateCriteriaResponse(
            request,
            itemType,
            moodLookup,
            themeLookup);

        var additionalMoodIds = request.AdditionalMoodIds
            .Order()
            .ToArray();

        var themeIds = request.ThemeIds
            .Order()
            .ToArray();

        var criteriaFingerprint =
            RecommendationCursorCodec.CreateCriteriaFingerprint(
                request.UserId,
                request.ItemTypeId,
                request.PrimaryMoodId,
                additionalMoodIds,
                themeIds);

        var cursorCodec = new RecommendationCursorCodec(
            jwtSettings.Value.SecretKey);

        RecommendationCursor? cursor = null;

        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            cursor = cursorCodec.Decode(request.Cursor);

            if (!string.Equals(
                    cursor.CriteriaFingerprint,
                    criteriaFingerprint,
                    StringComparison.Ordinal))
            {
                throw new BadRequestException(
                    "نشانگر صفحه‌بندی با معیارهای پیشنهاد فعلی مطابقت ندارد.");
            }
        }

        var additionalMoodCount = additionalMoodIds.Length;
        var themeCount = themeIds.Length;
        var additionalMoodDivisor = MaximumWeight
            * Math.Max(additionalMoodCount, 1);
        var themeDivisor = MaximumWeight * Math.Max(themeCount, 1);
        var activeCoefficientSum = PrimaryMoodCoefficient
            + (additionalMoodCount > 0
                ? AdditionalMoodCoefficient
                : 0m)
            + (themeCount > 0
                ? ThemeCoefficient
                : 0m);

        var additionalMoodScoresQuery = dbContext.ItemMoodWeights
            .AsNoTracking()
            .Where(weight =>
                additionalMoodIds.Contains(weight.MoodId)
                && weight.Mood.IsActive)
            .GroupBy(weight => weight.MediaItemId)
            .Select(group => new
            {
                MediaItemId = group.Key,
                Score = group.Sum(weight => weight.WeightValue)
                    / additionalMoodDivisor
            });

        var themeScoresQuery = dbContext.ItemThemeWeights
            .AsNoTracking()
            .Where(weight =>
                themeIds.Contains(weight.ThemeId)
                && weight.Theme.IsActive)
            .GroupBy(weight => weight.MediaItemId)
            .Select(group => new
            {
                MediaItemId = group.Key,
                Score = group.Sum(weight => weight.WeightValue)
                    / themeDivisor
            });

        var primaryCandidatesQuery = dbContext.ItemMoodWeights
            .AsNoTracking()
            .Where(weight =>
                weight.MoodId == request.PrimaryMoodId
                && weight.WeightValue >= NeutralWeight
                && weight.Mood.IsActive
                && weight.MediaItem.Status
                && weight.MediaItem.ItemTypeId == request.ItemTypeId
                && weight.MediaItem.ItemType.IsActive
                && !dbContext.Experiences.Any(experience =>
                    experience.UserId == request.UserId
                    && experience.MediaItemId == weight.MediaItemId));

        var componentsQuery =
            from primaryWeight in primaryCandidatesQuery
            join additionalMoodScore in additionalMoodScoresQuery
                on primaryWeight.MediaItemId
                equals additionalMoodScore.MediaItemId
                into additionalMoodScores
            from additionalMoodScore in additionalMoodScores.DefaultIfEmpty()
            join themeScore in themeScoresQuery
                on primaryWeight.MediaItemId equals themeScore.MediaItemId
                into themeScores
            from themeScore in themeScores.DefaultIfEmpty()
            select new
            {
                primaryWeight.MediaItemId,
                PrimaryMoodScore =
                    primaryWeight.WeightValue / MaximumWeight,
                PrimaryExperienceCount = primaryWeight.ExperienceCount,
                AdditionalMoodScore =
                    (decimal?)additionalMoodScore.Score ?? 0m,
                ThemeScore = (decimal?)themeScore.Score ?? 0m
            };

        var rankedQuery = componentsQuery
            .Select(candidate => new
            {
                candidate.MediaItemId,
                MediaItemKey = candidate.MediaItemId.ToString(),
                candidate.PrimaryMoodScore,
                candidate.PrimaryExperienceCount,
                FinalScore = (
                    PrimaryMoodCoefficient
                        * candidate.PrimaryMoodScore
                    + (additionalMoodCount > 0
                        ? AdditionalMoodCoefficient
                            * candidate.AdditionalMoodScore
                        : 0m)
                    + (themeCount > 0
                        ? ThemeCoefficient * candidate.ThemeScore
                        : 0m))
                    / activeCoefficientSum
            });

        if (cursor is not null)
        {
            rankedQuery = rankedQuery.Where(candidate =>
                candidate.FinalScore < cursor.FinalScore
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.PrimaryMoodScore
                        < cursor.PrimaryMoodScore)
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.PrimaryMoodScore
                        == cursor.PrimaryMoodScore
                    && candidate.PrimaryExperienceCount
                        < cursor.PrimaryExperienceCount)
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.PrimaryMoodScore
                        == cursor.PrimaryMoodScore
                    && candidate.PrimaryExperienceCount
                        == cursor.PrimaryExperienceCount
                    && candidate.MediaItemKey.CompareTo(
                        cursor.MediaItemKey) > 0));
        }

        var candidates = await rankedQuery
            .OrderByDescending(candidate => candidate.FinalScore)
            .ThenByDescending(candidate => candidate.PrimaryMoodScore)
            .ThenByDescending(candidate =>
                candidate.PrimaryExperienceCount)
            .ThenBy(candidate => candidate.MediaItemKey)
            .Take(request.PageSize + 1)
            .Select(candidate => new RankedCandidate(
                candidate.MediaItemId,
                candidate.MediaItemKey,
                candidate.PrimaryMoodScore,
                candidate.PrimaryExperienceCount,
                candidate.FinalScore))
            .ToListAsync(cancellationToken);

        var hasMore = candidates.Count > request.PageSize;
        var pageCandidates = candidates
            .Take(request.PageSize)
            .ToArray();

        if (pageCandidates.Length == 0)
        {
            return new GetRecommendationsResponse(
                criteria,
                [],
                null,
                false,
                "NoEligibleItems");
        }

        var items = await HydrateResultsAsync(
            pageCandidates,
            itemType,
            request.PrimaryMoodId,
            moodLookup[request.PrimaryMoodId],
            additionalMoodIds,
            themeIds,
            cancellationToken);

        var nextCursor = hasMore
            ? cursorCodec.Encode(new RecommendationCursor(
                criteriaFingerprint,
                pageCandidates[^1].FinalScore,
                pageCandidates[^1].PrimaryMoodScore,
                pageCandidates[^1].PrimaryExperienceCount,
                pageCandidates[^1].MediaItemKey))
            : null;

        return new GetRecommendationsResponse(
            criteria,
            items,
            nextCursor,
            hasMore,
            null);
    }

    private async Task<ItemTypeProjection> GetActiveItemTypeAsync(
        int itemTypeId,
        CancellationToken cancellationToken)
    {
        var itemType = await dbContext.ItemTypes
            .AsNoTracking()
            .Where(item => item.Id == itemTypeId && item.IsActive)
            .Select(item => new ItemTypeProjection(
                item.Id,
                item.Name))
            .FirstOrDefaultAsync(cancellationToken);

        return itemType
            ?? throw new BadRequestException(
                "نوع محتوای انتخاب‌شده وجود ندارد یا غیرفعال است.");
    }

    private async Task<IReadOnlyDictionary<int, string>> GetActiveMoodsAsync(
        GetRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var moodIds = request.AdditionalMoodIds
            .Append(request.PrimaryMoodId)
            .Distinct()
            .ToArray();

        var moods = await dbContext.Moods
            .AsNoTracking()
            .Where(mood =>
                moodIds.Contains(mood.Id)
                && mood.IsActive)
            .Select(mood => new NamedProjection(
                mood.Id,
                mood.Name))
            .ToListAsync(cancellationToken);

        if (moods.Count != moodIds.Length)
        {
            throw new BadRequestException(
                "یک یا چند حالت احساسی وجود ندارند یا غیرفعال هستند.");
        }

        return moods.ToDictionary(mood => mood.Id, mood => mood.Name);
    }

    private async Task<IReadOnlyDictionary<int, string>> GetActiveThemesAsync(
        IReadOnlyCollection<int> themeIds,
        CancellationToken cancellationToken)
    {
        if (themeIds.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        var themes = await dbContext.Themes
            .AsNoTracking()
            .Where(theme =>
                themeIds.Contains(theme.Id)
                && theme.IsActive)
            .Select(theme => new NamedProjection(
                theme.Id,
                theme.Name))
            .ToListAsync(cancellationToken);

        if (themes.Count != themeIds.Count)
        {
            throw new BadRequestException(
                "یک یا چند تم وجود ندارند یا غیرفعال هستند.");
        }

        return themes.ToDictionary(theme => theme.Id, theme => theme.Name);
    }

    private static RecommendationCriteriaResponse CreateCriteriaResponse(
        GetRecommendationsQuery request,
        ItemTypeProjection itemType,
        IReadOnlyDictionary<int, string> moodLookup,
        IReadOnlyDictionary<int, string> themeLookup)
        => new(
            new RecommendationNamedValueResponse(
                itemType.Id,
                itemType.Name),
            new RecommendationNamedValueResponse(
                request.PrimaryMoodId,
                moodLookup[request.PrimaryMoodId]),
            request.AdditionalMoodIds
                .Select(id => new RecommendationNamedValueResponse(
                    id,
                    moodLookup[id]))
                .ToArray(),
            request.ThemeIds
                .Select(id => new RecommendationNamedValueResponse(
                    id,
                    themeLookup[id]))
                .ToArray());

    private async Task<IReadOnlyList<RecommendationItemResponse>>
        HydrateResultsAsync(
            IReadOnlyCollection<RankedCandidate> candidates,
            ItemTypeProjection itemType,
            int primaryMoodId,
            string primaryMoodName,
            IReadOnlyCollection<int> additionalMoodIds,
            IReadOnlyCollection<int> themeIds,
            CancellationToken cancellationToken)
    {
        var mediaItemIds = candidates
            .Select(candidate => candidate.MediaItemId)
            .ToArray();

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
            itemType.Name,
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

        var additionalMoodIdSet = additionalMoodIds.ToHashSet();
        var themeIdSet = themeIds.ToHashSet();

        var matchedMoodsByItem = CreateMatchedValues(
            moodRows,
            additionalMoodIdSet);

        var matchedThemesByItem = CreateMatchedValues(
            themeRows,
            themeIdSet);

        var primaryMood = new RecommendationNamedValueResponse(
            primaryMoodId,
            primaryMoodName);

        var results = new List<RecommendationItemResponse>(
            candidates.Count);

        foreach (var candidate in candidates)
        {
            if (!mediaItems.TryGetValue(
                    candidate.MediaItemId,
                    out var mediaItem))
            {
                continue;
            }

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
                    GetValuesOrEmpty(matchedMoodsByItem, mediaItem.Id),
                    GetValuesOrEmpty(matchedThemesByItem, mediaItem.Id),
                    RecommendationMode),
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

    private sealed record ItemTypeProjection(int Id, string Name);

    private sealed record NamedProjection(int Id, string Name);

    private sealed record RankedCandidate(
        Guid MediaItemId,
        string MediaItemKey,
        decimal PrimaryMoodScore,
        int PrimaryExperienceCount,
        decimal FinalScore);

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
