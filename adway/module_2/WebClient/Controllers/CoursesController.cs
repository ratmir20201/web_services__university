using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Controllers;

public class CoursesController : Controller
{
    private readonly IUniversityApiClient _apiClient;
    private readonly UniversityApiOptions _apiOptions;

    public CoursesController(
        IUniversityApiClient apiClient,
        IOptions<UniversityApiOptions> apiOptions)
    {
        _apiClient = apiClient;
        _apiOptions = apiOptions.Value;
    }

    public async Task<IActionResult> Index(string? search, int version = 2, CancellationToken cancellationToken = default)
    {
        var model = new CoursesIndexViewModel
        {
            Version = version == 1 ? 1 : 2,
            Search = search
        };

        if (model.Version == 1)
        {
            var result = await _apiClient.GetCoursesV1Async(search, cancellationToken);
            model.CoursesV1 = result.Data ?? new List<CourseV1Model>();
            model.ErrorMessage = result.ErrorMessage;
        }
        else
        {
            var result = await _apiClient.GetCoursesV2Async(search, cancellationToken);
            model.CoursesV2 = result.Data ?? new List<CourseV2Model>();
            model.ErrorMessage = result.ErrorMessage;
        }

        return View(model);
    }

    public async Task<IActionResult> Details(int id, int version = 2, CancellationToken cancellationToken = default)
    {
        var model = new CourseDetailsViewModel
        {
            Version = version == 1 ? 1 : 2
        };

        if (model.Version == 1)
        {
            var result = await _apiClient.GetCourseV1Async(id, cancellationToken);
            model.CourseV1 = result.Data;
            model.ErrorMessage = result.ErrorMessage;

            if (result.StatusCode == StatusCodes.Status404NotFound)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }
        else
        {
            var result = await _apiClient.GetCourseV2Async(id, cancellationToken);
            model.CourseV2 = result.Data;
            model.ErrorMessage = result.ErrorMessage;

            if (result.StatusCode == StatusCodes.Status404NotFound)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }

        return View(model);
    }

    public IActionResult Browser()
    {
        return View(new BrowserDemoViewModel
        {
            ApiBaseUrl = _apiOptions.BaseUrl
        });
    }
}
