using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Auth;
using WebApi.Common;
using WebApi.DTO.Students;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(
        IStudentService studentService,
        IAuthorizationService authorizationService,
        ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = Policies.AdminOrTeacher)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation(
            "Получен запрос на получение списка всех студентов");

        var students =
            await _studentService.GetAllAsync();

        _logger.LogInformation(
            "Список студентов успешно получен");

        return Ok(
            new ReturnResult<IEnumerable<StudentDto>>
            {
                StatusCode = 200,
                IsSuccess = true,
                Result = students,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation(
            "Получен запрос на получение студента с ID {StudentId}",
            id);

        await _authorizationService.EnsureAuthorizedAsync(
            User,
            new StudentResource(id),
            Policies.StudentDataAccess);

        var student =
            await _studentService.GetByIdAsync(id);

        _logger.LogInformation(
            "Студент с ID {StudentId} успешно получен",
            id);

        return Ok(
            new ReturnResult<StudentDto>
            {
                StatusCode = 200,
                IsSuccess = true,
                Result = student,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpGet("{id}/courses")]
    public async Task<IActionResult> GetCourses(int id)
    {
        _logger.LogInformation(
            "Получен запрос на получение курсов студента с ID {StudentId}",
            id);

        await _authorizationService.EnsureAuthorizedAsync(
            User,
            new StudentResource(id),
            Policies.StudentDataAccess);

        var courses =
            await _studentService.GetCoursesAsync(id);

        _logger.LogInformation(
            "Курсы студента с ID {StudentId} успешно получены",
            id);

        return Ok(
            new ReturnResult<IEnumerable<StudentCourseDto>>
            {
                StatusCode = 200,
                IsSuccess = true,
                Result = courses,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Create(
        StudentCreateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на создание студента");

        var student =
            await _studentService.CreateAsync(dto);

        _logger.LogInformation(
            "Студент успешно создан. ID: {StudentId}",
            student.Id);

        return StatusCode(
            201,
            new ReturnResult<StudentDto>
            {
                StatusCode = 201,
                IsSuccess = true,
                Result = student,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Update(
        int id,
        StudentUpdateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на обновление студента с ID {StudentId}",
            id);

        await _studentService.UpdateAsync(id, dto);

        _logger.LogInformation(
            "Студент с ID {StudentId} успешно обновлен",
            id);

        return Ok(
            new ReturnResult<object>
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
            "Получен запрос на удаление студента с ID {StudentId}",
            id);

        await _studentService.DeleteAsync(id);

        _logger.LogInformation(
            "Студент с ID {StudentId} успешно удален",
            id);

        return Ok(
            new ReturnResult<object>
            {
                StatusCode = 200,
                IsSuccess = true,
                Result = null,
                TraceId = HttpContext.TraceIdentifier
            });
    }
}
