using Microsoft.AspNetCore.Mvc;
using WebApi.Common;
using WebApi.DTO.Enrollments;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(
        IEnrollmentService enrollmentService,
        ILogger<EnrollmentsController> logger)
    {
        _enrollmentService = enrollmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation(
            "Получен запрос на получение списка всех записей о зачислении");

        var enrollments =
            await _enrollmentService.GetAllAsync();

        _logger.LogInformation(
            "Список записей о зачислении успешно получен");

        return Ok(
            new ReturnResult<IEnumerable<EnrollmentDto>>
            {
                StatusCode = 200,
                IsSuccess = true,
                Result = enrollments,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation(
            "Получен запрос на получение записи о зачислении с ID {EnrollmentId}",
            id);

        var enrollment =
            await _enrollmentService.GetByIdAsync(id);

        _logger.LogInformation(
            "Запись о зачислении с ID {EnrollmentId} успешно получена",
            id);

        return Ok(
            new ReturnResult<EnrollmentDto>
            {
                StatusCode = 200,
                IsSuccess = true,
                Result = enrollment,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        EnrollmentCreateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на зачисление студента {StudentId} на курс {CourseId}",
            dto.StudentId,
            dto.CourseId);

        var enrollment =
            await _enrollmentService.CreateAsync(dto);

        _logger.LogInformation(
            "Зачисление успешно создано. ID: {EnrollmentId}",
            enrollment.Id);

        return StatusCode(
            201,
            new ReturnResult<EnrollmentDto>
            {
                StatusCode = 201,
                IsSuccess = true,
                Result = enrollment,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        EnrollmentUpdateDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на обновление записи о зачислении с ID {EnrollmentId}",
            id);

        await _enrollmentService.UpdateAsync(id, dto);

        _logger.LogInformation(
            "Запись о зачислении с ID {EnrollmentId} успешно обновлена",
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
            "Получен запрос на удаление записи о зачислении с ID {EnrollmentId}",
            id);

        await _enrollmentService.DeleteAsync(id);

        _logger.LogInformation(
            "Запись о зачислении с ID {EnrollmentId} успешно удалена",
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
