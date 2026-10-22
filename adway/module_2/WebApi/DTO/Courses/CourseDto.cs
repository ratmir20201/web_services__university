namespace WebApi.DTO.Courses;

public class CourseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Credits { get; set; }

    public int TeacherId { get; set; }

    public DateTime CreatedAt { get; set; }
}