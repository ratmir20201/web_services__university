using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Auth;
using WebApi.Models;

namespace WebApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role {Role} created", role);
            }
        }

        if (!configuration.GetValue<bool>("Seed:Enabled"))
        {
            return;
        }

        var password = configuration["Seed:DefaultPassword"];

        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:DefaultPassword is not configured, test users were not created");
            return;
        }

        var petrov = await EnsureTeacherAsync(context, "Иван", "Петров", "ivan.petrov@example.com", "Информатика");
        var smirnova = await EnsureTeacherAsync(context, "Анна", "Смирнова", "anna.smirnova@example.com", "Математика");

        var ivanov = await EnsureStudentAsync(context, "Алексей", "Иванов", "alexey.ivanov@example.com", new DateTime(2004, 5, 15, 0, 0, 0, DateTimeKind.Utc));
        var sokolova = await EnsureStudentAsync(context, "Мария", "Соколова", "maria.sokolova@example.com", new DateTime(2003, 11, 20, 0, 0, 0, DateTimeKind.Utc));

        var databases = await EnsureCourseAsync(context, "Базы данных", "Проектирование реляционных баз данных и язык SQL", 5, petrov.Id);
        var calculus = await EnsureCourseAsync(context, "Математический анализ", "Пределы, производные и интегралы", 4, smirnova.Id);

        await EnsureEnrollmentAsync(context, ivanov.Id, databases.Id, 85);
        await EnsureEnrollmentAsync(context, ivanov.Id, calculus.Id, 78);
        await EnsureEnrollmentAsync(context, sokolova.Id, databases.Id, 92);
        await EnsureEnrollmentAsync(context, sokolova.Id, calculus.Id, 88);

        await EnsureUserAsync(userManager, logger, "admin", "admin@university.local", password, Roles.Admin, null, null);
        await EnsureUserAsync(userManager, logger, "teacher.petrov", "petrov@university.local", password, Roles.Teacher, null, petrov.Id);
        await EnsureUserAsync(userManager, logger, "teacher.smirnova", "smirnova@university.local", password, Roles.Teacher, null, smirnova.Id);
        await EnsureUserAsync(userManager, logger, "student.ivanov", "ivanov@university.local", password, Roles.Student, ivanov.Id, null);
        await EnsureUserAsync(userManager, logger, "student.sokolova", "sokolova@university.local", password, Roles.Student, sokolova.Id, null);
    }

    private static async Task<Teacher> EnsureTeacherAsync(
        ApplicationDbContext context,
        string firstName,
        string lastName,
        string email,
        string department)
    {
        var teacher = await context.Teachers.FirstOrDefaultAsync(x => x.Email == email);

        if (teacher != null)
        {
            return teacher;
        }

        teacher = new Teacher
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Department = department
        };

        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();

        return teacher;
    }

    private static async Task<Student> EnsureStudentAsync(
        ApplicationDbContext context,
        string firstName,
        string lastName,
        string email,
        DateTime birthDate)
    {
        var student = await context.Students.FirstOrDefaultAsync(x => x.Email == email);

        if (student != null)
        {
            return student;
        }

        student = new Student
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            BirthDate = birthDate,
            CreatedAt = DateTime.UtcNow
        };

        context.Students.Add(student);
        await context.SaveChangesAsync();

        return student;
    }

    private static async Task<Course> EnsureCourseAsync(
        ApplicationDbContext context,
        string name,
        string description,
        int credits,
        int teacherId)
    {
        var course = await context.Courses.FirstOrDefaultAsync(x => x.Name == name && x.TeacherId == teacherId);

        if (course != null)
        {
            return course;
        }

        course = new Course
        {
            Name = name,
            Description = description,
            Credits = credits,
            TeacherId = teacherId,
            CreatedAt = DateTime.UtcNow
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        return course;
    }

    private static async Task EnsureEnrollmentAsync(
        ApplicationDbContext context,
        int studentId,
        int courseId,
        decimal grade)
    {
        var exists = await context.Enrollments.AnyAsync(x => x.StudentId == studentId && x.CourseId == courseId);

        if (exists)
        {
            return;
        }

        context.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            Grade = grade
        });

        await context.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string userName,
        string email,
        string password,
        string role,
        int? studentId,
        int? teacherId)
    {
        var user = await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                StudentId = studentId,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                logger.LogWarning(
                    "Test user {UserName} was not created: {Errors}",
                    userName,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Test user {UserName} created", userName);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
