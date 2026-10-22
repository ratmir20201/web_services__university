using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repositories.Interfaces;

namespace WebApi.Repositories.Implementations;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        return await _context.Enrollments.ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Enrollments
            .Where(x => x.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByTeacherIdAsync(int teacherId)
    {
        return await _context.Enrollments
            .Where(x => x.Course!.TeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
    {
        return await _context.Enrollments.FindAsync(id);
    }

    public async Task<Enrollment?> GetByIdWithCourseAsync(int id)
    {
        return await _context.Enrollments
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Enrollment enrollment)
    {
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> StudentExistsAsync(int studentId)
    {
        return await _context.Students.AnyAsync(x => x.Id == studentId);
    }

    public async Task<bool> CourseExistsAsync(int courseId)
    {
        return await _context.Courses.AnyAsync(x => x.Id == courseId);
    }

    public async Task<bool> ExistsAsync(int studentId, int courseId)
    {
        return await _context.Enrollments
            .AnyAsync(x =>
                x.StudentId == studentId &&
                x.CourseId == courseId);
    }
}
