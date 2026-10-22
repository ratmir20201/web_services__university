using Microsoft.AspNetCore.Mvc;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers;

public class AccountController : Controller
{
    public const string TokenSessionKey = "AccessToken";
    public const string UserNameSessionKey = "UserName";

    private readonly IUniversityApiClient _apiClient;

    public AccountController(IUniversityApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _apiClient.LoginAsync(model.UserName, model.Password, cancellationToken);

        if (!result.IsSuccess || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не удалось выполнить вход.");
            model.Password = string.Empty;
            return View(model);
        }

        HttpContext.Session.SetString(TokenSessionKey, result.Data.AccessToken);
        HttpContext.Session.SetString(UserNameSessionKey, result.Data.UserName);

        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Courses");
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString(TokenSessionKey);

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new ProfileViewModel();
        var profile = await _apiClient.GetProfileAsync(token, cancellationToken);

        if (profile.StatusCode == StatusCodes.Status401Unauthorized)
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Срок действия входа истек. Войдите снова.";
            return RedirectToAction(nameof(Login));
        }

        model.Profile = profile.Data;
        model.ErrorMessage = profile.ErrorMessage;

        if (profile.Data?.StudentId is int studentId)
        {
            var grades = await _apiClient.GetStudentCoursesAsync(studentId, token, cancellationToken);
            model.Grades = grades.Data ?? new List<StudentCourseModel>();
            model.ErrorMessage ??= grades.ErrorMessage;
        }

        return View(model);
    }
}
