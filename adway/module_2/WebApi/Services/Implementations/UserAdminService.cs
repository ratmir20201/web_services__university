using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Auth;
using WebApi.Common.Exceptions;
using WebApi.DTO.Admin;
using WebApi.DTO.Auth;
using WebApi.Models;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;

    public UserAdminService(
        UserManager<ApplicationUser> userManager,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository)
    {
        _userManager = userManager;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<IEnumerable<UserProfileDto>> GetUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(x => x.UserName)
            .ToListAsync();

        var result = new List<UserProfileDto>();

        foreach (var user in users)
        {
            result.Add(await BuildProfileAsync(user));
        }

        return result;
    }

    public IEnumerable<string> GetRoles()
    {
        return Roles.All;
    }

    public async Task<UserProfileDto> AssignRoleAsync(string userId, string role)
    {
        var normalizedRole = NormalizeRole(role);
        var user = await GetUserAsync(userId);

        if (!await _userManager.IsInRoleAsync(user, normalizedRole))
        {
            var result = await _userManager.AddToRoleAsync(user, normalizedRole);
            EnsureSucceeded(result);
            await _userManager.UpdateSecurityStampAsync(user);
        }

        return await BuildProfileAsync(user);
    }

    public async Task<UserProfileDto> RemoveRoleAsync(
        string userId,
        string role,
        string currentUserId)
    {
        var normalizedRole = NormalizeRole(role);

        if (userId == currentUserId && normalizedRole == Roles.Admin)
        {
            throw new ApiException(
                400,
                "CANNOT_REMOVE_OWN_ADMIN_ROLE",
                "Administrator cannot remove the Admin role from own account.");
        }

        var user = await GetUserAsync(userId);

        if (await _userManager.IsInRoleAsync(user, normalizedRole))
        {
            var result = await _userManager.RemoveFromRoleAsync(user, normalizedRole);
            EnsureSucceeded(result);
            await _userManager.UpdateSecurityStampAsync(user);
        }

        return await BuildProfileAsync(user);
    }

    public async Task<UserProfileDto> LinkAsync(string userId, LinkUserDto dto)
    {
        var user = await GetUserAsync(userId);

        if (dto.StudentId.HasValue)
        {
            if (!await _studentRepository.ExistsAsync(dto.StudentId.Value))
            {
                throw new ApiException(404, "STUDENT_NOT_FOUND", "Student not found.");
            }

            var linked = await _userManager.Users.AnyAsync(x =>
                x.StudentId == dto.StudentId && x.Id != userId);

            if (linked)
            {
                throw new ApiException(
                    409,
                    "STUDENT_ALREADY_LINKED",
                    "Student is already linked to another account.");
            }
        }

        if (dto.TeacherId.HasValue)
        {
            if (!await _teacherRepository.ExistsAsync(dto.TeacherId.Value))
            {
                throw new ApiException(404, "TEACHER_NOT_FOUND", "Teacher not found.");
            }

            var linked = await _userManager.Users.AnyAsync(x =>
                x.TeacherId == dto.TeacherId && x.Id != userId);

            if (linked)
            {
                throw new ApiException(
                    409,
                    "TEACHER_ALREADY_LINKED",
                    "Teacher is already linked to another account.");
            }
        }

        user.StudentId = dto.StudentId;
        user.TeacherId = dto.TeacherId;

        EnsureSucceeded(await _userManager.UpdateAsync(user));

        return await BuildProfileAsync(user);
    }

    private async Task<ApplicationUser> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new ApiException(404, "USER_NOT_FOUND", "User not found.");
        }

        return user;
    }

    private static string NormalizeRole(string role)
    {
        var match = Roles.All.FirstOrDefault(x =>
            string.Equals(x, role, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            throw new ApiException(
                400,
                "INVALID_ROLE",
                $"Role must be one of: {string.Join(", ", Roles.All)}.");
        }

        return match;
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new ApiException(
                400,
                "IDENTITY_ERROR",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task<UserProfileDto> BuildProfileAsync(ApplicationUser user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = await _userManager.GetRolesAsync(user),
            StudentId = user.StudentId,
            TeacherId = user.TeacherId
        };
    }
}
