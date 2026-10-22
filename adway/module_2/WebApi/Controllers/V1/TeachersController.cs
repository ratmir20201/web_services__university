using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Auth;
using WebApi.Common;
using WebApi.DTO.Teachers;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Authorize]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(
        ITeacherService teacherService,
        ILogger<TeachersController> logger)
    {
        _teacherService = teacherService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation(
            "Получен запрос на получение списка всех преподавателей");

        var teachers =
            await _teacherService.GetAllAsync();

        _logger.LogInformation(
            "Список преподавателей успешно получен");

        return Ok(new ReturnResult<IEnumerable<TeacherDto>>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = teachers,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation(
            "Получен запрос на получение преподавателя с ID {TeacherId}",
            id);

        var teacher =
            await _teacherService.GetByIdAsync(id);

        _logger.LogInformation(
            "Преподаватель с ID {TeacherId} успешно получен",
            id);

        return Ok(new ReturnResult<TeacherDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = teacher,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Create(
        TeacherCreateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на создание преподавателя");

        var teacher =
            await _teacherService.CreateAsync(dto);

        _logger.LogInformation(
            "Преподаватель успешно создан. ID: {TeacherId}",
            teacher.Id);

        return StatusCode(
            201,
            new ReturnResult<TeacherDto>
            {
                StatusCode = 201,
                IsSuccess = true,
                Result = teacher,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Update(
        int id,
        TeacherUpdateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на обновление преподавателя с ID {TeacherId}",
            id);

        await _teacherService.UpdateAsync(id, dto);

        _logger.LogInformation(
            "Преподаватель с ID {TeacherId} успешно обновлен",
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
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation(
            "Получен запрос на удаление преподавателя с ID {TeacherId}",
            id);

        await _teacherService.DeleteAsync(id);

        _logger.LogInformation(
            "Преподаватель с ID {TeacherId} успешно удален",
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
