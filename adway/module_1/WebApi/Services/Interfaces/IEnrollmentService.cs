using WebApi.DTO.Enrollments;

namespace WebApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentDto>> GetAllAsync();

    Task<EnrollmentDto?> GetByIdAsync(int id);

    Task<EnrollmentDto> CreateAsync(EnrollmentCreateDto dto);

    Task<bool> UpdateAsync(int id, EnrollmentUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}