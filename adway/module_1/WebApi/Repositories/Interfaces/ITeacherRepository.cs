using WebApi.Models;

namespace WebApi.Repositories.Interfaces;

public interface ITeacherRepository
{
    Task<IEnumerable<Teacher>> GetAllAsync();

    Task<Teacher?> GetByIdAsync(int id);

    Task AddAsync(Teacher teacher);

    Task UpdateAsync(Teacher teacher);

    Task DeleteAsync(Teacher teacher);

    Task<bool> ExistsAsync(int id);

    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}