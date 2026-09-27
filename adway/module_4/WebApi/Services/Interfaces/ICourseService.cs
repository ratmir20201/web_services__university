using WebApi.DTO.Courses;

namespace WebApi.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync(string? search = null);

    Task<CourseDto?> GetByIdAsync(int id);

    Task<CourseDto> CreateAsync(CourseCreateDto dto);

    Task<bool> UpdateAsync(int id, CourseUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}