using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EmotionClient.Models;

namespace EmotionClient.Services;

public sealed class EmotionApiService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<EmotionApiResult<IReadOnlyList<RecommendationLookupOption>>> GetMoodsAsync(
        CancellationToken cancellationToken)
        => GetLookupAsync("api/moods?isActive=true", cancellationToken);

    public Task<EmotionApiResult<IReadOnlyList<RecommendationLookupOption>>> GetThemesAsync(
        CancellationToken cancellationToken)
        => GetLookupAsync("api/themes?isActive=true", cancellationToken);

    public async Task<EmotionApiResult<RecommendationResponse>> GetMoodRecommendationsAsync(
        MoodRecommendationSelectionModel selection,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            selection.ItemTypeId,
            selection.PrimaryMoodId,
            AdditionalMoodIds = selection.AdditionalMoodIds.Distinct().ToArray(),
            ThemeIds = selection.ThemeIds.Distinct().ToArray(),
            PageSize = 1,
            selection.Cursor
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/recommendations")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return await FailureAsync<RecommendationResponse>(response, cancellationToken);

            var data = await response.Content.ReadFromJsonAsync<RecommendationResponse>(
                JsonOptions,
                cancellationToken);

            return data is null
                ? EmotionApiResult<RecommendationResponse>.Failure(502, "The recommendation service returned an empty response.")
                : EmotionApiResult<RecommendationResponse>.Success(data, (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return EmotionApiResult<RecommendationResponse>.Failure(504, "The recommendation service took too long to respond. Please try again.");
        }
        catch (HttpRequestException)
        {
            return EmotionApiResult<RecommendationResponse>.Failure(503, "The recommendation service is currently unavailable. Please try again shortly.");
        }
    }

    public async Task<EmotionApiResult<IReadOnlyList<MediaItemOption>>> GetMediaItemsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/media-items", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return await FailureAsync<IReadOnlyList<MediaItemOption>>(response, cancellationToken);

            var data = await response.Content.ReadFromJsonAsync<List<MediaItemOption>>(
                JsonOptions,
                cancellationToken);

            return EmotionApiResult<IReadOnlyList<MediaItemOption>>.Success(
                data ?? [],
                (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return EmotionApiResult<IReadOnlyList<MediaItemOption>>.Failure(
                504,
                "The media catalog took too long to respond.");
        }
        catch (HttpRequestException)
        {
            return EmotionApiResult<IReadOnlyList<MediaItemOption>>.Failure(
                503,
                "The media catalog is currently unavailable.");
        }
    }

    public async Task<EmotionApiResult<IReadOnlyList<UserExperienceSummary>>> GetMyExperiencesAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthenticatedRequest(
            HttpMethod.Get,
            "api/experiences/me",
            accessToken);

        return await SendAsync<IReadOnlyList<UserExperienceSummary>>(request, cancellationToken);
    }

    public async Task<EmotionApiResult<CreateExperienceResponse>> CreateExperienceAsync(
        ExperienceEntryModel entry,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            entry.MediaItemId,
            entry.Score,
            Note = (string?)null,
            MoodIds = entry.MoodIds.Distinct().ToArray(),
            ThemeIds = entry.ThemeIds.Distinct().ToArray()
        };

        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "api/experiences",
            accessToken,
            payload);

        return await SendAsync<CreateExperienceResponse>(request, cancellationToken);
    }

    public async Task<EmotionApiResult<ExperienceRecommendationResponse>> GetExperienceRecommendationsAsync(
        ExperienceRecommendationSelectionModel selection,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            selection.ExperienceId,
            PageSize = 1,
            selection.Cursor
        };

        using var request = CreateAuthenticatedRequest(
            HttpMethod.Post,
            "api/recommendations/by-experience",
            accessToken,
            payload);

        return await SendAsync<ExperienceRecommendationResponse>(request, cancellationToken);
    }

    private async Task<EmotionApiResult<IReadOnlyList<RecommendationLookupOption>>> GetLookupAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return await FailureAsync<IReadOnlyList<RecommendationLookupOption>>(response, cancellationToken);

            var data = await response.Content.ReadFromJsonAsync<List<RecommendationLookupOption>>(
                JsonOptions,
                cancellationToken);

            return EmotionApiResult<IReadOnlyList<RecommendationLookupOption>>.Success(
                data ?? [],
                (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return EmotionApiResult<IReadOnlyList<RecommendationLookupOption>>.Failure(
                504,
                "The recommendation service took too long to respond.");
        }
        catch (HttpRequestException)
        {
            return EmotionApiResult<IReadOnlyList<RecommendationLookupOption>>.Failure(
                503,
                "The recommendation service is currently unavailable.");
        }
    }

    private static HttpRequestMessage CreateAuthenticatedRequest(
        HttpMethod method,
        string path,
        string accessToken,
        object? payload = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (payload is not null)
            request.Content = JsonContent.Create(payload);

        return request;
    }

    private async Task<EmotionApiResult<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return await FailureAsync<T>(response, cancellationToken);

            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return data is null
                ? EmotionApiResult<T>.Failure(502, "The recommendation service returned an empty response.")
                : EmotionApiResult<T>.Success(data, (int)response.StatusCode);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return EmotionApiResult<T>.Failure(
                504,
                "The recommendation service took too long to respond. Please try again.");
        }
        catch (HttpRequestException)
        {
            return EmotionApiResult<T>.Failure(
                503,
                "The recommendation service is currently unavailable. Please try again shortly.");
        }
    }

    private static async Task<EmotionApiResult<T>> FailureAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string? serviceMessage = null;

        try
        {
            var error = await response.Content.ReadFromJsonAsync<EmotionApiError>(
                JsonOptions,
                cancellationToken);
            serviceMessage = error?.Message;
        }
        catch (JsonException)
        {
            // A stable English fallback is returned below when the service body is not JSON.
        }

        var message = (int)response.StatusCode switch
        {
            400 => "Some of your selections are no longer valid. Please review them and try again.",
            401 => "Your session has expired. Please sign in again.",
            404 => "The requested recommendation data could not be found.",
            409 => "You already have an experience saved for this title.",
            422 => "No recommendation could be created from those selections.",
            _ => "We could not complete your request. Please try again."
        };

        if (!string.IsNullOrWhiteSpace(serviceMessage) && IsEnglish(serviceMessage))
            message = serviceMessage;

        return EmotionApiResult<T>.Failure((int)response.StatusCode, message);
    }

    private static bool IsEnglish(string value)
        => value.All(character => character <= 127);

    private sealed class EmotionApiError
    {
        public string? Message { get; set; }
    }
}
