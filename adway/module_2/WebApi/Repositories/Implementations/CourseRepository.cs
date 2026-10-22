using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repositories.Interfaces;

namespace WebApi.Repositories.Implementations;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetAllAsync(string? search = null)
    {
        return await ApplySearch(_context.Courses.AsQueryable(), search)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetAllWithDetailsAsync(string? search = null)
    {
        return await ApplySearch(WithDetails(), search)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses.FindAsync(id);
    }

    public async Task<Course?> GetByIdWithDetailsAsync(int id)
    {
        return await WithDetails()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Course course)
    {
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Courses.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> TeacherExistsAsync(int teacherId)
    {
        return await _context.Teachers.AnyAsync(x => x.Id == teacherId);
    }

    private IQueryable<Course> WithDetails()
    {
        return _context.Courses
            .AsNoTracking()
            .Include(x => x.Teacher)
            .Include(x => x.Enrollments);
    }

    private static IQueryable<Course> ApplySearch(
        IQueryable<Course> courses,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return courses;
        }

        return courses.Where(x =>
            x.Name.Contains(search) ||
            x.Description.Contains(search));
    }
}
