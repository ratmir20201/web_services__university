namespace WebClient.Models;

public class ApiEnvelope<T>
{
    public int StatusCode { get; set; }

    public bool IsSuccess { get; set; }

    public T? Result { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? TraceId { get; set; }
}

public class CourseV1Model
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Credits { get; set; }

    public int TeacherId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CourseTeacherModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
}

public class CourseV2Model
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Credits { get; set; }

    public DateTime CreatedAt { get; set; }

    public CourseTeacherModel? Teacher { get; set; }

    public int EnrolledStudentsCount { get; set; }
}

public class LoginResponseModel
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string UserName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}

public class UserProfileModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();

    public int? StudentId { get; set; }

    public int? TeacherId { get; set; }
}

public class StudentCourseModel
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int Credits { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public decimal? Grade { get; set; }
}
