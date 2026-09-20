using WebApi.Models;

namespace WebApi.Services;

public class StudentService : IStudentService
{
    private readonly List<Student> students = new()
    {
        new Student
        {
            Id = 1,
            Name = "Ratmir",
            Group = "adw1"
        },

        new Student
        {
            Id = 2,
            Name = "Timur",
            Group = "adw2"
        },

        new Student
        {
            Id = 3,
            Name = "Andrey",
            Group = "adw1"
        }
    };

    public IEnumerable<Student> GetAll()
    {
        return students;
    }

    public Student? GetById(int id)
    {
        return students.FirstOrDefault(s => s.Id == id);
    }
}