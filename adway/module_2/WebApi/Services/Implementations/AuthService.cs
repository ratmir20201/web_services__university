using Microsoft.AspNetCore.Identity;
using WebApi.Auth;
using WebApi.Common.Exceptions;
using WebApi.DTO.Auth;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userManager.FindByNameAsync(dto.UserName) != null)
        {
            throw new ApiException(
                409,
                "USERNAME_ALREADY_EXISTS",
                "User with this user name already exists.");
        }

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
        {
            throw new ApiException(
                409,
                "EMAIL_ALREADY_EXISTS",
                "User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            throw new ApiException(
                400,
                "REGISTRATION_FAILED",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, Roles.Student);

        return await BuildProfileAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);

        if (user == null)
        {
            throw InvalidCredentials();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            throw new ApiException(
                423,
                "ACCOUNT_LOCKED",
                "Account is temporarily locked because of several failed login attempts.");
        }

        if (!result.Succeeded)
        {
            throw InvalidCredentials();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return _tokenService.CreateAccessToken(user, roles);
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new ApiException(
                401,
                "UNAUTHORIZED",
                "User account no longer exists.");
        }

        return await BuildProfileAsync(user);
    }

    private async Task<UserProfileDto> BuildProfileAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles,
            StudentId = user.StudentId,
            TeacherId = user.TeacherId
        };
    }

    private static ApiException InvalidCredentials()
    {
        return new ApiException(
            401,
            "INVALID_CREDENTIALS",
            "Invalid user name or password.");
    }
}
