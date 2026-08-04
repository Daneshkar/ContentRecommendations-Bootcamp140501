using EmotionClient.Models;
using EmotionClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmotionClient.Controllers;

public class AccountController : Controller
{
    private readonly AuthApiService _authApi;

    public AccountController(AuthApiService authApi)
    {
        _authApi = authApi;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _authApi.LoginAsync(model.Username, model.Password);

        if (!result.IsSuccess)
        {
            ViewBag.Error = result.Message ?? "ورود ناموفق";
            return View(model);
        }

        ForwardAuthCookies(result.Cookies);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var result = await _authApi.RegisterAsync(model);

        if (!result.IsSuccess)
        {
            ViewBag.Error = result.Message ?? "ثبت‌نام ناموفق";
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.Mobile))
        {
            TempData["RegisteredMobile"] = model.Mobile;
            TempData["SuccessMessage"] = "ثبت‌نام با موفقیت انجام شد. حالا شماره موبایل خود را فعال کنید.";
            return RedirectToAction("VerifyMobile");
        }

        TempData["Info"] = "ثبت‌نام با موفقیت انجام شد. اکنون می‌توانید وارد شوید.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult VerifyMobile()
    {
        ViewBag.Mobile = TempData["RegisteredMobile"];
        ViewBag.Message = TempData["SuccessMessage"];
        return View();
    }

    [HttpPost("send-otp-ajax")]
    public async Task<IActionResult> SendOtpAjax([FromBody] SendOtpRequest request)
    {
        var result = await _authApi.SendOtpAsync(request.Mobile);
        return Json(result);
    }

    [HttpPost("verify-otp-ajax")]
    public async Task<IActionResult> VerifyOtpAjax([FromBody] VerifyOtpRequest request)
    {
        var result = await _authApi.VerifyOtpAsync(request.Mobile, request.Code);
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["access_token"];
        if (!string.IsNullOrEmpty(token))
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            try { await httpClient.PostAsync($"{GetBaseUrl()}/api/auth/logout", null); } catch { }
        }

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var token = Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login");

        var result = await _authApi.GetProfileAsync(token);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Message ?? "خطا در دریافت اطلاعات پروفایل";
            return RedirectToAction("Login");
        }

        return View(result.Data);
    }

    private void ForwardAuthCookies(Dictionary<string, string> cookies)
    {
        foreach (var (name, value) in cookies)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = name == "access_token"
                    ? DateTimeOffset.UtcNow.AddMinutes(15)
                    : DateTimeOffset.UtcNow.AddDays(7)
            };

            if (name == "refresh_token")
                options.Path = "/api/auth/refresh";

            Response.Cookies.Append(name, value, options);
        }
    }

    private string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}";
    }
}

public record SendOtpRequest(string Mobile);
public record VerifyOtpRequest(string Mobile, string Code);
