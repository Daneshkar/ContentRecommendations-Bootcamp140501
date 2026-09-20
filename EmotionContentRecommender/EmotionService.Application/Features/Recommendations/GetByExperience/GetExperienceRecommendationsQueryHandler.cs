using EmotionService.Application.Features.Recommendations.Get;
using EmotionService.Infrastructure.Exceptions;
using EmotionService.Infrastructure.Jwt;
using EmotionService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EmotionService.Application.Features.Recommendations.GetByExperience;

public sealed class GetExperienceRecommendationsQueryHandler(
    ApplicationDbContext dbContext,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<
        GetExperienceRecommendationsQuery,
        GetExperienceRecommendationsResponse>
{
    private const decimal NeutralWeight = 0.05m;
    private const decimal MaximumWeight = 0.10m;
    private const decimal MoodCoefficient = 0.80m;
    private const decimal ThemeCoefficient = 0.20m;
    private const string RecommendationMode = "ExperienceBased";

    public async Task<GetExperienceRecommendationsResponse> Handle(
        GetExperienceRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var source = await LoadSourceExperienceAsync(
            request,
            cancellationToken);

        ValidateSourceExperience(source);

        var moods = await LoadActiveExperienceMoodsAsync(
            source.Id,
            cancellationToken);

        if (moods.Count == 0)
        {
            throw new BadRequestException(
                "این تجربه هیچ حالت احساسی فعال برای پیشنهاد محتوا ندارد.");
        }

        var themes = await LoadActiveExperienceThemesAsync(
            source.Id,
            cancellationToken);

        var moodIds = moods.Select(mood => mood.Id).ToArray();
        var themeIds = themes.Select(theme => theme.Id).ToArray();
        var criteria = new ExperienceRecommendationCriteriaResponse(
            source.Id,
            new ExperienceRecommendationSourceItemResponse(
                source.MediaItemId,
                source.MediaItemName),
            new RecommendationNamedValueResponse(
                source.ItemTypeId,
                source.ItemTypeName),
            moods.Select(mood => new RecommendationNamedValueResponse(
                    mood.Id,
                    mood.Name))
                .ToArray(),
            themes.Select(theme => new RecommendationNamedValueResponse(
                    theme.Id,
                    theme.Name))
                .ToArray());

        var criteriaFingerprint =
            RecommendationCursorCodec.CreateExperienceCriteriaFingerprint(
                request.UserId,
                source.Id,
                source.ItemTypeId,
                moodIds,
                themeIds);

        var cursorCodec = new RecommendationCursorCodec(
            jwtSettings.Value.SecretKey);
        var cursor = DecodeAndValidateCursor(
            request.Cursor,
            cursorCodec,
            criteriaFingerprint);

        var moodDivisor = MaximumWeight * moodIds.Length;
        var themeDivisor = MaximumWeight * Math.Max(themeIds.Length, 1);

        var moodScoresQuery = dbContext.ItemMoodWeights
            .AsNoTracking()
            .Where(weight =>
                moodIds.Contains(weight.MoodId)
                && weight.Mood.IsActive)
            .GroupBy(weight => weight.MediaItemId)
            .Select(group => new
            {
                MediaItemId = group.Key,
                Score = group.Sum(weight => weight.WeightValue)
                    / moodDivisor,
                AggregateSupport = group.Sum(weight => weight.ExperienceCount)
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

        var eligibleMediaItemIdsQuery = dbContext.ItemMoodWeights
            .AsNoTracking()
            .Where(weight =>
                moodIds.Contains(weight.MoodId)
                && weight.Mood.IsActive
                && weight.WeightValue >= NeutralWeight)
            .Select(weight => weight.MediaItemId)
            .Distinct();

        var eligibleItemsQuery = dbContext.MediaItems
            .AsNoTracking()
            .Where(item =>
                item.Status
                && item.ItemType.IsActive
                && item.ItemTypeId == source.ItemTypeId
                && item.Id != source.MediaItemId
                && eligibleMediaItemIdsQuery.Contains(item.Id)
                && !dbContext.Experiences.Any(experience =>
                    experience.UserId == request.UserId
                    && experience.MediaItemId == item.Id));

        var componentsQuery =
            from item in eligibleItemsQuery
            join moodScore in moodScoresQuery
                on item.Id equals moodScore.MediaItemId
            join themeScore in themeScoresQuery
                on item.Id equals themeScore.MediaItemId
                into themeScores
            from themeScore in themeScores.DefaultIfEmpty()
            select new
            {
                MediaItemId = item.Id,
                MoodScore = moodScore.Score,
                moodScore.AggregateSupport,
                ThemeScore = (decimal?)themeScore.Score ?? 0m
            };

        var hasThemes = themeIds.Length > 0;
        var rankedQuery = componentsQuery.Select(candidate => new
        {
            candidate.MediaItemId,
            MediaItemKey = candidate.MediaItemId.ToString(),
            candidate.MoodScore,
            candidate.AggregateSupport,
            FinalScore = hasThemes
                ? MoodCoefficient * candidate.MoodScore
                    + ThemeCoefficient * candidate.ThemeScore
                : candidate.MoodScore
        });

        if (cursor is not null)
        {
            rankedQuery = rankedQuery.Where(candidate =>
                candidate.FinalScore < cursor.FinalScore
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.MoodScore < cursor.SecondaryScore)
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.MoodScore == cursor.SecondaryScore
                    && candidate.AggregateSupport
                        < cursor.AggregateSupport)
                || (candidate.FinalScore == cursor.FinalScore
                    && candidate.MoodScore == cursor.SecondaryScore
                    && candidate.AggregateSupport
                        == cursor.AggregateSupport
                    && candidate.MediaItemKey.CompareTo(
                        cursor.MediaItemKey) > 0));
        }

        var candidates = await rankedQuery
            .OrderByDescending(candidate => candidate.FinalScore)
            .ThenByDescending(candidate => candidate.MoodScore)
            .ThenByDescending(candidate => candidate.AggregateSupport)
            .ThenBy(candidate => candidate.MediaItemKey)
            .Take(request.PageSize + 1)
            .Select(candidate => new RankedCandidate(
                candidate.MediaItemId,
                candidate.MediaItemKey,
                candidate.MoodScore,
                candidate.AggregateSupport,
                candidate.FinalScore))
            .ToListAsync(cancellationToken);

        var hasMore = candidates.Count > request.PageSize;
        var pageCandidates = candidates.Take(request.PageSize).ToArray();

        if (pageCandidates.Length == 0)
        {
            return new GetExperienceRecommendationsResponse(
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
                source.ItemTypeName,
                null,
                moodIds,
                themeIds,
                RecommendationMode,
                cancellationToken);

        var nextCursor = hasMore
            ? cursorCodec.Encode(new RecommendationCursor(
                criteriaFingerprint,
                pageCandidates[^1].FinalScore,
                pageCandidates[^1].MoodScore,
                pageCandidates[^1].AggregateSupport,
                pageCandidates[^1].MediaItemKey))
            : null;

        return new GetExperienceRecommendationsResponse(
            criteria,
            items,
            nextCursor,
            hasMore,
            null);
    }

    private async Task<SourceExperienceProjection> LoadSourceExperienceAsync(
        GetExperienceRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var source = await dbContext.Experiences
            .AsNoTracking()
            .Where(experience =>
                experience.Id == request.ExperienceId
                && experience.UserId == request.UserId)
            .Select(experience => new SourceExperienceProjection(
                experience.Id,
                experience.Score,
                experience.MediaItemId,
                experience.MediaItem.Name,
                experience.MediaItem.Status,
                experience.MediaItem.ItemTypeId,
                experience.MediaItem.ItemType.Name,
                experience.MediaItem.ItemType.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        return source
            ?? throw new NotFoundException(
                "تجربه‌ی مورد نظر یافت نشد.");
    }

    private static void ValidateSourceExperience(
        SourceExperienceProjection source)
    {
        if (source.Score < 4)
        {
            throw new BadRequestException(
                "فقط تجربه‌های مثبت با امتیاز ۴ یا ۵ می‌توانند مبنای پیشنهاد باشند.");
        }

        if (!source.MediaItemIsActive || !source.ItemTypeIsActive)
        {
            throw new BadRequestException(
                "محتوا یا نوع محتوای تجربه‌ی مبنا غیرفعال است.");
        }
    }

    private async Task<IReadOnlyList<NamedProjection>>
        LoadActiveExperienceMoodsAsync(
            Guid experienceId,
            CancellationToken cancellationToken)
        => await dbContext.ExperienceMoods
            .AsNoTracking()
            .Where(item =>
                item.ExperienceId == experienceId
                && item.Mood.IsActive)
            .OrderBy(item => item.MoodId)
            .Select(item => new NamedProjection(
                item.MoodId,
                item.Mood.Name))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<NamedProjection>>
        LoadActiveExperienceThemesAsync(
            Guid experienceId,
            CancellationToken cancellationToken)
        => await dbContext.ExperienceThemes
            .AsNoTracking()
            .Where(item =>
                item.ExperienceId == experienceId
                && item.Theme.IsActive)
            .OrderBy(item => item.ThemeId)
            .Select(item => new NamedProjection(
                item.ThemeId,
                item.Theme.Name))
            .ToListAsync(cancellationToken);

    private static RecommendationCursor? DecodeAndValidateCursor(
        string? encodedCursor,
        RecommendationCursorCodec cursorCodec,
        string criteriaFingerprint)
    {
        if (string.IsNullOrWhiteSpace(encodedCursor))
        {
            return null;
        }

        var cursor = cursorCodec.Decode(encodedCursor);

        if (!string.Equals(
                cursor.CriteriaFingerprint,
                criteriaFingerprint,
                StringComparison.Ordinal))
        {
            throw new BadRequestException(
                "نشانگر صفحه‌بندی با معیارهای پیشنهاد فعلی مطابقت ندارد.");
        }

        return cursor;
    }

    private sealed record SourceExperienceProjection(
        Guid Id,
        int Score,
        Guid MediaItemId,
        string MediaItemName,
        bool MediaItemIsActive,
        int ItemTypeId,
        string ItemTypeName,
        bool ItemTypeIsActive);

    private sealed record NamedProjection(int Id, string Name);

    private sealed record RankedCandidate(
        Guid MediaItemId,
        string MediaItemKey,
        decimal MoodScore,
        int AggregateSupport,
        decimal FinalScore);
}
