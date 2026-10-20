using Microsoft.AspNetCore.Mvc;
using WebApi.Common;
using WebApi.DTO.Courses;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(
        ICourseService courseService,
        ILogger<CoursesController> logger)
    {
        _courseService = courseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? search = null)
    {
        _logger.LogInformation(
            "Получен запрос на получение списка курсов. Поиск: {Search}",
            search ?? "не задан");

        var courses =
            await _courseService.GetAllAsync(search);

        _logger.LogInformation(
            "Список курсов успешно получен");

        return Ok(new ReturnResult<IEnumerable<CourseDto>>
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
            "Получен запрос на получение курса с ID {CourseId}",
            id);

        var course =
            await _courseService.GetByIdAsync(id);

        _logger.LogInformation(
            "Курс с ID {CourseId} успешно получен",
            id);

        return Ok(new ReturnResult<CourseDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = course,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CourseCreateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на создание курса с названием {CourseName}",
            dto.Name);

        var course =
            await _courseService.CreateAsync(dto);

        _logger.LogInformation(
            "Курс успешно создан. ID: {CourseId}",
            course.Id);

        return StatusCode(
            201,
            new ReturnResult<CourseDto>
            {
                StatusCode = 201,
                IsSuccess = true,
                Result = course,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        CourseUpdateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на обновление курса с ID {CourseId}",
            id);

        await _courseService.UpdateAsync(id, dto);

        _logger.LogInformation(
            "Курс с ID {CourseId} успешно обновлен",
            id);

        return Ok(new ReturnResult<object>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = null,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation(
            "Получен запрос на удаление курса с ID {CourseId}",
            id);

        await _courseService.DeleteAsync(id);

        _logger.LogInformation(
            "Курс с ID {CourseId} успешно удален",
            id);

        return Ok(new ReturnResult<object>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = null,
            TraceId = HttpContext.TraceIdentifier
        });
    }
}
