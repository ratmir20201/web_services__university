using Microsoft.AspNetCore.Authorization;

namespace WebApi.Auth;

public record StudentResource(int StudentId);

public record EnrollmentResource(int StudentId, int CourseTeacherId);

public record CourseResource(int CourseId, int TeacherId);

public class StudentDataRequirement : IAuthorizationRequirement
{
}

public class EnrollmentReadRequirement : IAuthorizationRequirement
{
}

public class CourseTeacherRequirement : IAuthorizationRequirement
{
}

public class StudentDataHandler
    : AuthorizationHandler<StudentDataRequirement, StudentResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentDataRequirement requirement,
        StudentResource resource)
    {
        var user = context.User;

        if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Teacher))
        {
            context.Succeed(requirement);
        }
        else if (user.IsInRole(Roles.Student) &&
                 user.GetStudentId() == resource.StudentId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class EnrollmentReadHandler
    : AuthorizationHandler<EnrollmentReadRequirement, EnrollmentResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EnrollmentReadRequirement requirement,
        EnrollmentResource resource)
    {
        var user = context.User;

        if (user.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }
        else if (user.IsInRole(Roles.Teacher) &&
                 user.GetTeacherId() == resource.CourseTeacherId)
        {
            context.Succeed(requirement);
        }
        else if (user.IsInRole(Roles.Student) &&
                 user.GetStudentId() == resource.StudentId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class CourseTeacherHandler
    : AuthorizationHandler<CourseTeacherRequirement, CourseResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseTeacherRequirement requirement,
        CourseResource resource)
    {
        var user = context.User;

        if (user.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
        }
        else if (user.IsInRole(Roles.Teacher) &&
                 user.GetTeacherId() == resource.TeacherId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
