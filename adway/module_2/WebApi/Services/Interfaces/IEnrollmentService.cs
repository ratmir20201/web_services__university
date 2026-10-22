using WebApi.DTO.Enrollments;

namespace WebApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentDto>> GetAllAsync();

    Task<IEnumerable<EnrollmentDto>> GetByStudentAsync(int studentId);

    Task<IEnumerable<EnrollmentDto>> GetByTeacherAsync(int teacherId);

    Task<EnrollmentDto?> GetByIdAsync(int id);

    Task<int> GetCourseTeacherIdAsync(int id);

    Task<EnrollmentDto> CreateAsync(EnrollmentCreateDto dto);

    Task<bool> UpdateAsync(int id, EnrollmentUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}
