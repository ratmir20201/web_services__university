using WebApi.DTO.Teachers;

namespace WebApi.Services.Interfaces;

public interface ITeacherService
{
    Task<IEnumerable<TeacherDto>> GetAllAsync();

    Task<TeacherDto?> GetByIdAsync(int id);

    Task<TeacherDto> CreateAsync(TeacherCreateDto dto);

    Task<bool> UpdateAsync(int id, TeacherUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}