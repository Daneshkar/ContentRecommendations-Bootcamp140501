using EmotionClient.Models;
using EmotionClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmotionClient.Controllers;

public sealed class HistoryController(EmotionApiService emotionApi) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var accessToken = Request.Cookies["access_token"];
        if (string.IsNullOrWhiteSpace(accessToken))
            return RedirectToLogin();

        var result = await emotionApi.GetMyExperiencesAsync(accessToken, cancellationToken);
        if (result.StatusCode == StatusCodes.Status401Unauthorized)
        {
            Response.Cookies.Delete("access_token");
            return RedirectToLogin();
        }

        return View(new ExperienceHistoryPageViewModel
        {
            Experiences = (result.Data ?? [])
                .OrderByDescending(item => item.CreatedAt)
                .ToArray(),
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage
        });
    }

    private IActionResult RedirectToLogin()
        => RedirectToAction(
            "Login",
            "Account",
            new { returnUrl = Url.Action(nameof(Index), "History") });
}
