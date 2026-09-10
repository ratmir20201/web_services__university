using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private static readonly List<Student> students = new()
    {
        new Student
        {
            Id = 1,
            Name = "Ayan",
            Age = 20
        },

        new Student
        {
            Id = 2,
            Name = "Dana",
            Age = 21
        }
    };

    [HttpGet]
    public ActionResult<List<Student>> GetStudents()
    {
        return Ok(students);
    }

    [HttpPost]
    public ActionResult<Student> AddStudent(Student student)
    {
        student.Id = students.Count + 1;

        students.Add(student);

        return Ok(student);
    }

    [HttpGet("{id}")]
    public ActionResult<Student> GetStudent(int id)
    {
        var student = students.FirstOrDefault(s => s.Id == id);

        if (student == null)
        {
            return NotFound();
        }

        return Ok(student);
    }
}