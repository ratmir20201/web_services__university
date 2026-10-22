using WebApi.DTO.Courses;
using WebApi.DTO.Courses.V2;

namespace WebApi.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync(string? search = null);

    Task<IEnumerable<CourseV2Dto>> GetAllV2Async(string? search = null);

    Task<CourseDto?> GetByIdAsync(int id);

    Task<CourseV2Dto> GetByIdV2Async(int id);

    Task<CourseDto> CreateAsync(CourseCreateDto dto);

    Task<bool> UpdateAsync(int id, CourseUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}
