namespace WebApi.Auth;

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminOrTeacher = "AdminOrTeacher";
    public const string StudentDataAccess = "StudentDataAccess";
    public const string EnrollmentReadAccess = "EnrollmentReadAccess";
    public const string CourseTeacherAccess = "CourseTeacherAccess";
}
