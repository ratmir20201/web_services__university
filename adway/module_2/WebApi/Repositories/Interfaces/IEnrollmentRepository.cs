using WebApi.Models;

namespace WebApi.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<IEnumerable<Enrollment>> GetAllAsync();

    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId);

    Task<IEnumerable<Enrollment>> GetByTeacherIdAsync(int teacherId);

    Task<Enrollment?> GetByIdAsync(int id);

    Task<Enrollment?> GetByIdWithCourseAsync(int id);

    Task AddAsync(Enrollment enrollment);

    Task UpdateAsync(Enrollment enrollment);

    Task DeleteAsync(Enrollment enrollment);

    Task<bool> StudentExistsAsync(int studentId);

    Task<bool> CourseExistsAsync(int courseId);

    Task<bool> ExistsAsync(int studentId, int courseId);
}
