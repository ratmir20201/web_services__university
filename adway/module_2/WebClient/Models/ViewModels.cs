using System.ComponentModel.DataAnnotations;

namespace WebClient.Models;

public class CoursesIndexViewModel
{
    public int Version { get; set; }

    public string? Search { get; set; }

    public List<CourseV1Model> CoursesV1 { get; set; } = new();

    public List<CourseV2Model> CoursesV2 { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

public class CourseDetailsViewModel
{
    public int Version { get; set; }

    public CourseV1Model? CourseV1 { get; set; }

    public CourseV2Model? CourseV2 { get; set; }

    public string? ErrorMessage { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    public UserProfileModel? Profile { get; set; }

    public List<StudentCourseModel> Grades { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

public class BrowserDemoViewModel
{
    public string ApiBaseUrl { get; set; } = string.Empty;
}
