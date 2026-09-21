using EmotionClient.Models;
using EmotionClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmotionClient.Controllers;

public sealed class RecommendationController(EmotionApiService emotionApi) : Controller
{
    private static readonly HashSet<string> FeaturedMoods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cozy", "Hopeful", "Peaceful", "Inspired",
        "Energetic", "Romantic", "Thoughtful", "Tense"
    };

    [HttpGet]
    public async Task<IActionResult> Mood(
        [FromQuery] MoodRecommendationSelectionModel selection,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out _))
            return RedirectToLogin();

        var page = await BuildMoodPageAsync(selection, null, cancellationToken);
        ViewData["FeaturedMoods"] = FeaturedMoods;
        return View(page);
    }

    [HttpPost]
    [ActionName(nameof(Mood))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FindMood(
        MoodRecommendationSelectionModel selection,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out var accessToken))
            return RedirectToLogin();

        selection.Cursor = null;
        var validationPage = await ValidateSelectionAsync(selection, cancellationToken);
        if (validationPage is not null)
        {
            ViewData["FeaturedMoods"] = FeaturedMoods;
            return View(validationPage);
        }

        var result = await emotionApi.GetMoodRecommendationsAsync(
            selection,
            accessToken,
            cancellationToken);

        if (result.StatusCode == StatusCodes.Status401Unauthorized)
            return ExpireSessionAndRedirect();

        if (!result.IsSuccess)
        {
            var page = await BuildMoodPageAsync(selection, result.ErrorMessage, cancellationToken);
            ViewData["FeaturedMoods"] = FeaturedMoods;
            return View(page);
        }

        return View("Result", new RecommendationResultViewModel
        {
            Selection = selection,
            Response = result.Data
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Next(
        MoodRecommendationSelectionModel selection,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out var accessToken))
            return RedirectToLogin();

        if (string.IsNullOrWhiteSpace(selection.Cursor))
            return RedirectToAction(nameof(Mood));

        var result = await emotionApi.GetMoodRecommendationsAsync(
            selection,
            accessToken,
            cancellationToken);

        if (result.StatusCode == StatusCodes.Status401Unauthorized)
            return ExpireSessionAndRedirect();

        return View("Result", new RecommendationResultViewModel
        {
            Selection = selection,
            Response = result.Data,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage
        });
    }

    private async Task<MoodRecommendationPageViewModel?> ValidateSelectionAsync(
        MoodRecommendationSelectionModel selection,
        CancellationToken cancellationToken)
    {
        var page = await BuildMoodPageAsync(selection, null, cancellationToken);
        if (page.LookupError is not null)
            return page;

        string? error = null;
        if (!RecommendationCatalog.IsKnownItemType(selection.ItemTypeId))
            error = "Choose what kind of recommendation you want.";
        else if (page.Moods.All(mood => mood.Id != selection.PrimaryMoodId))
            error = "Choose one mood to guide your recommendation.";
        else if (selection.AdditionalMoodIds.Count != selection.AdditionalMoodIds.Distinct().Count())
            error = "Choose each additional mood only once.";
        else if (selection.AdditionalMoodIds.Distinct().Count() > 2)
            error = "Choose no more than two additional moods.";
        else if (selection.AdditionalMoodIds.Contains(selection.PrimaryMoodId))
            error = "The primary mood cannot also be an additional mood.";
        else if (selection.AdditionalMoodIds.Any(id => page.Moods.All(mood => mood.Id != id)))
            error = "One or more selected moods are no longer available.";
        else if (selection.ThemeIds.Distinct().Count() > 5)
            error = "Choose no more than five themes.";
        else if (selection.ThemeIds.Any(id => page.Themes.All(theme => theme.Id != id)))
            error = "One or more selected themes are no longer available.";

        return error is null
            ? null
            : new MoodRecommendationPageViewModel
            {
                Selection = selection,
                Moods = page.Moods,
                Themes = page.Themes,
                SubmissionError = error
            };
    }

    private async Task<MoodRecommendationPageViewModel> BuildMoodPageAsync(
        MoodRecommendationSelectionModel selection,
        string? submissionError,
        CancellationToken cancellationToken)
    {
        var moodsTask = emotionApi.GetMoodsAsync(cancellationToken);
        var themesTask = emotionApi.GetThemesAsync(cancellationToken);
        await Task.WhenAll(moodsTask, themesTask);

        var moodsResult = await moodsTask;
        var themesResult = await themesTask;
        var lookupError = !moodsResult.IsSuccess
            ? moodsResult.ErrorMessage
            : !themesResult.IsSuccess
                ? themesResult.ErrorMessage
                : null;

        return new MoodRecommendationPageViewModel
        {
            Selection = selection,
            Moods = (moodsResult.Data ?? [])
                .Where(option => option.IsActive)
                .OrderByDescending(option => FeaturedMoods.Contains(option.Name))
                .ThenBy(option => option.Name)
                .ToArray(),
            Themes = (themesResult.Data ?? [])
                .Where(option => option.IsActive)
                .OrderBy(option => option.Name)
                .ToArray(),
            LookupError = lookupError,
            SubmissionError = submissionError
        };
    }

    [HttpGet]
    public async Task<IActionResult> Experience(CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out var accessToken))
            return RedirectToLogin(nameof(Experience));

        var page = await BuildExperienceStartPageAsync(accessToken, null, cancellationToken);
        if (page is null)
            return ExpireSessionAndRedirect(nameof(Experience));

        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShareExperience(
        ExperienceStartSelectionModel selection,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out var accessToken))
            return RedirectToLogin(nameof(Experience));

        var startPage = await BuildExperienceStartPageAsync(accessToken, null, cancellationToken);
        if (startPage is null)
            return ExpireSessionAndRedirect(nameof(Experience));

        var mediaItem = startPage.MediaItems.FirstOrDefault(item => item.Id == selection.MediaItemId);
        if (mediaItem is null)
        {
            return View("Experience", new ExperienceStartPageViewModel
            {
                Selection = selection,
                MediaItems = startPage.MediaItems,
                ErrorMessage = "Choose an available title to continue."
            });
        }

        var page = await BuildExperienceEntryPageAsync(
            new ExperienceEntryModel { MediaItemId = mediaItem.Id },
            mediaItem,
            null,
            cancellationToken);

        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExperience(
        ExperienceEntryModel entry,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out var accessToken))
            return RedirectToLogin(nameof(Experience));

        var catalogResult = await emotionApi.GetMediaItemsAsync(cancellationToken);
        var mediaItem = catalogResult.Data?.FirstOrDefault(item => item.Id == entry.MediaItemId);
        if (!catalogResult.IsSuccess || mediaItem is null)
        {
            return View("ShareExperience", new ExperienceEntryPageViewModel
            {
                Entry = entry,
                MediaItem = mediaItem ?? new MediaItemOption { Id = entry.MediaItemId, Name = "Selected title" },
                ErrorMessage = catalogResult.ErrorMessage ?? "The selected title is no longer available."
            });
        }

        var page = await BuildExperienceEntryPageAsync(entry, mediaItem, null, cancellationToken);
        var validationError = ValidateExperienceEntry(page);
        if (validationError is not null)
        {
            return View("ShareExperience", new ExperienceEntryPageViewModel
            {
                Entry = entry,
                MediaItem = mediaItem,
                Moods = page.Moods,
                Themes = page.Themes,
                ErrorMessage = validationError
            });
        }

        var createResult = await emotionApi.CreateExperienceAsync(
            entry,
            accessToken,
            cancellationToken);

        if (createResult.StatusCode == StatusCodes.Status401Unauthorized)
            return ExpireSessionAndRedirect(nameof(Experience));

        if (!createResult.IsSuccess || createResult.Data is null)
        {
            return View("ShareExperience", new ExperienceEntryPageViewModel
            {
                Entry = entry,
                MediaItem = mediaItem,
                Moods = page.Moods,
                Themes = page.Themes,
                ErrorMessage = createResult.ErrorMessage
            });
        }

        if (entry.Score <= 3)
        {
            return View("ExperienceThanks", new ExperienceThanksViewModel
            {
                MediaItemName = mediaItem.Name,
                Score = entry.Score
            });
        }

        var selection = new ExperienceRecommendationSelectionModel
        {
            ExperienceId = createResult.Data.Id
        };
        var recommendationResult = await emotionApi.GetExperienceRecommendationsAsync(
            selection,
            accessToken,
            cancellationToken);

        if (recommendationResult.StatusCode == StatusCodes.Status401Unauthorized)
            return ExpireSessionAndRedirect(nameof(Experience));

        return View("Result", new RecommendationResultViewModel
        {
            ExperienceSelection = selection,
            ExperienceResponse = recommendationResult.Data,
            ErrorMessage = recommendationResult.IsSuccess ? null : recommendationResult.ErrorMessage
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NextExperience(
        ExperienceRecommendationSelectionModel selection,
        CancellationToken cancellationToken)
    {
        if (!TryGetAccessToken(out var accessToken))
            return RedirectToLogin(nameof(Experience));

        if (selection.ExperienceId == Guid.Empty)
            return RedirectToAction(nameof(Experience));

        var result = await emotionApi.GetExperienceRecommendationsAsync(
            selection,
            accessToken,
            cancellationToken);

        if (result.StatusCode == StatusCodes.Status401Unauthorized)
            return ExpireSessionAndRedirect(nameof(Experience));

        return View("Result", new RecommendationResultViewModel
        {
            ExperienceSelection = selection,
            ExperienceResponse = result.Data,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage
        });
    }

    private async Task<ExperienceStartPageViewModel?> BuildExperienceStartPageAsync(
        string accessToken,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var mediaItemsTask = emotionApi.GetMediaItemsAsync(cancellationToken);
        var experiencesTask = emotionApi.GetMyExperiencesAsync(accessToken, cancellationToken);
        await Task.WhenAll(mediaItemsTask, experiencesTask);

        var mediaItemsResult = await mediaItemsTask;
        var experiencesResult = await experiencesTask;
        if (experiencesResult.StatusCode == StatusCodes.Status401Unauthorized)
            return null;

        var existingMediaIds = (experiencesResult.Data ?? [])
            .Select(item => item.MediaItemId)
            .ToHashSet();
        var loadError = errorMessage
            ?? (!mediaItemsResult.IsSuccess ? mediaItemsResult.ErrorMessage : null)
            ?? (!experiencesResult.IsSuccess ? experiencesResult.ErrorMessage : null);

        return new ExperienceStartPageViewModel
        {
            MediaItems = (mediaItemsResult.Data ?? [])
                .Where(item => item.IsActive
                    && RecommendationCatalog.IsKnownItemType(item.ItemTypeId)
                    && !existingMediaIds.Contains(item.Id))
                .OrderBy(item => item.Name)
                .ToArray(),
            ErrorMessage = loadError
        };
    }

    private async Task<ExperienceEntryPageViewModel> BuildExperienceEntryPageAsync(
        ExperienceEntryModel entry,
        MediaItemOption mediaItem,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var moodsTask = emotionApi.GetMoodsAsync(cancellationToken);
        var themesTask = emotionApi.GetThemesAsync(cancellationToken);
        await Task.WhenAll(moodsTask, themesTask);

        var moodsResult = await moodsTask;
        var themesResult = await themesTask;
        var lookupError = errorMessage
            ?? (!moodsResult.IsSuccess ? moodsResult.ErrorMessage : null)
            ?? (!themesResult.IsSuccess ? themesResult.ErrorMessage : null);

        return new ExperienceEntryPageViewModel
        {
            Entry = entry,
            MediaItem = mediaItem,
            Moods = (moodsResult.Data ?? [])
                .Where(item => item.IsActive)
                .OrderByDescending(item => FeaturedMoods.Contains(item.Name))
                .ThenBy(item => item.Name)
                .ToArray(),
            Themes = (themesResult.Data ?? [])
                .Where(item => item.IsActive)
                .OrderBy(item => item.Name)
                .ToArray(),
            ErrorMessage = lookupError
        };
    }

    private static string? ValidateExperienceEntry(ExperienceEntryPageViewModel page)
    {
        var entry = page.Entry;
        if (page.ErrorMessage is not null)
            return page.ErrorMessage;
        if (entry.Score is < 1 or > 5)
            return "Choose a rating between 1 and 5.";
        if (entry.MoodIds.Count != entry.MoodIds.Distinct().Count()
            || entry.MoodIds.Count is < 1 or > 3)
        {
            return "Choose between one and three different moods.";
        }
        if (entry.MoodIds.Any(id => page.Moods.All(mood => mood.Id != id)))
            return "One or more selected moods are no longer available.";
        if (entry.ThemeIds.Count != entry.ThemeIds.Distinct().Count()
            || entry.ThemeIds.Count > 5)
        {
            return "Choose no more than five different themes.";
        }
        if (entry.ThemeIds.Any(id => page.Themes.All(theme => theme.Id != id)))
            return "One or more selected themes are no longer available.";

        return null;
    }

    private bool TryGetAccessToken(out string accessToken)
    {
        accessToken = Request.Cookies["access_token"] ?? string.Empty;
        return !string.IsNullOrWhiteSpace(accessToken);
    }

    private IActionResult RedirectToLogin(string actionName = nameof(Mood))
        => RedirectToAction(
            "Login",
            "Account",
            new { returnUrl = Url.Action(actionName, "Recommendation") });

    private IActionResult ExpireSessionAndRedirect(string actionName = nameof(Mood))
    {
        Response.Cookies.Delete("access_token");
        return RedirectToLogin(actionName);
    }
}
