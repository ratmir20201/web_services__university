namespace WebApi.DTO.Courses.V2;

public class CourseV2Dto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Credits { get; set; }

    public DateTime CreatedAt { get; set; }

    public CourseTeacherV2Dto? Teacher { get; set; }

    public int EnrolledStudentsCount { get; set; }
}
