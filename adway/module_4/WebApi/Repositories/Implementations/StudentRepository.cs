using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repositories.Interfaces;

namespace WebApi.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students.ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students.FindAsync(id);
    }

    public async Task<Student?> GetByIdWithCoursesAsync(int id)
    {
        return await _context.Students
            .Include(x => x.Enrollments)
            .ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Student student)
    {
        _context.Students.Remove(student);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Students
            .AnyAsync(x => x.Id == id);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeId = null)
    {
        return await _context.Students
            .AnyAsync(x =>
                x.Email == email &&
                (!excludeId.HasValue ||
                 x.Id != excludeId.Value));
    }
}