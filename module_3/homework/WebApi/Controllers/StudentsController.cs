using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    private readonly ILogger<StudentsController> _logger;

    public StudentsController(
        IStudentService studentService,
        ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Student>> GetAll()
    {
        _logger.LogInformation("Получение всех студнетов");

        var students = _studentService.GetAll();

        return Ok(students);
    }

    [HttpGet("{id}")]
    public ActionResult<Student> GetById(int id)
    {
        _logger.LogInformation(
            "Получение студента с ID {StudentId}",
            id);

        var student = _studentService.GetById(id);

        if (student == null)
        {
            _logger.LogWarning(
                "Студент с ID {StudentId} не найден",
                id);

            return NotFound();
        }

        return Ok(student);
    }
}