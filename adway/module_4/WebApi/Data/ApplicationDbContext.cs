using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        modelBuilder.Entity<Student>()
            .Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Student>()
            .Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Student>()
            .Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Teacher>()
            .Property(t => t.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Teacher>()
            .Property(t => t.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Teacher>()
            .Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Course>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Course>()
            .Property(c => c.Credits)
            .IsRequired();
    }
}