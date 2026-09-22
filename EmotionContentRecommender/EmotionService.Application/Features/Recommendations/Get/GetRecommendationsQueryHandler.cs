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
                        < cursor.SecondaryScore)
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.PrimaryMoodScore
                        == cursor.SecondaryScore
                    && candidate.PrimaryExperienceCount
                        < cursor.AggregateSupport)
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.PrimaryMoodScore
                        == cursor.SecondaryScore
                    && candidate.PrimaryExperienceCount
                        == cursor.AggregateSupport
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

        var items = await new RecommendationResultHydrator(dbContext)
            .HydrateAsync(
                pageCandidates.Select(candidate => candidate.MediaItemId)
                    .ToArray(),
                itemType.Name,
                new RecommendationNamedValueResponse(
                    request.PrimaryMoodId,
                    moodLookup[request.PrimaryMoodId]),
                additionalMoodIds,
                themeIds,
                RecommendationMode,
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

    private sealed record ItemTypeProjection(int Id, string Name);

    private sealed record NamedProjection(int Id, string Name);

    private sealed record RankedCandidate(
        Guid MediaItemId,
        string MediaItemKey,
        decimal PrimaryMoodScore,
        int PrimaryExperienceCount,
        decimal FinalScore);
}
