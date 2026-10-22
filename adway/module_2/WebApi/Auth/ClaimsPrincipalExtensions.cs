using System.Security.Claims;

namespace WebApi.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(AppClaimTypes.UserId);
    }

    public static int? GetStudentId(this ClaimsPrincipal user)
    {
        return ParseInt(user.FindFirstValue(AppClaimTypes.StudentId));
    }

    public static int? GetTeacherId(this ClaimsPrincipal user)
    {
        return ParseInt(user.FindFirstValue(AppClaimTypes.TeacherId));
    }

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }
}
