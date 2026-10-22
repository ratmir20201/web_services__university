namespace WebApi.DTO.Students;

public class StudentCourseDto
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int Credits { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public decimal? Grade { get; set; }
}