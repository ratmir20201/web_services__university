using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common;
using WebApi.DTO.Courses.V2;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/courses")]
[AllowAnonymous]
public class CoursesV2Controller : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesV2Controller> _logger;

    public CoursesV2Controller(
        ICourseService courseService,
        ILogger<CoursesV2Controller> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? search = null)
    {
        _logger.LogInformation(
            "Получен запрос на получение списка курсов (v2). Поиск: {Search}",
            search ?? "не задан");

        var courses =
            await _courseService.GetAllV2Async(search);

        return Ok(new ReturnResult<IEnumerable<CourseV2Dto>>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = courses,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation(
            "Получен запрос на получение курса с ID {CourseId} (v2)",
            id);

        var course =
            await _courseService.GetByIdV2Async(id);

        return Ok(new ReturnResult<CourseV2Dto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = course,
            TraceId = HttpContext.TraceIdentifier
        });
    }
}
