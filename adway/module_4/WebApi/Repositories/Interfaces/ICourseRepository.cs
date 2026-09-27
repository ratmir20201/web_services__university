using WebApi.Models;

namespace WebApi.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllAsync(string? search = null);

    Task<Course?> GetByIdAsync(int id);

    Task AddAsync(Course course);

    Task UpdateAsync(Course course);

    Task DeleteAsync(Course course);

    Task<bool> ExistsAsync(int id);

    Task<bool> TeacherExistsAsync(int teacherId);
}