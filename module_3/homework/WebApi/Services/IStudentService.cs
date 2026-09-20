using WebApi.Models;

namespace WebApi.Services;

public interface IStudentService
{
    IEnumerable<Student> GetAll();

    Student? GetById(int id);
}