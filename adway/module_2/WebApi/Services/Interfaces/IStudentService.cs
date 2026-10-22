using WebApi.DTO.Students;

namespace WebApi.Services.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllAsync();

    Task<StudentDto?> GetByIdAsync(int id);

    Task<IEnumerable<StudentCourseDto>> GetCoursesAsync(int id);

    Task<StudentDto> CreateAsync(
        StudentCreateDto dto);

    Task<bool> UpdateAsync(
        int id,
        StudentUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}