using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Common.Exceptions;

namespace WebApi.Auth;

public static class AuthorizationServiceExtensions
{
    public static async Task EnsureAuthorizedAsync(
        this IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        object resource,
        string policy)
    {
        var result = await authorizationService.AuthorizeAsync(
            user,
            resource,
            policy);

        if (!result.Succeeded)
        {
            throw new ApiException(
                403,
                "FORBIDDEN",
                "You do not have permission to access this resource.");
        }
    }
}
