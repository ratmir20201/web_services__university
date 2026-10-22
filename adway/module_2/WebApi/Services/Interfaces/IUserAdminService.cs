using WebApi.DTO.Admin;
using WebApi.DTO.Auth;

namespace WebApi.Services.Interfaces;

public interface IUserAdminService
{
    Task<IEnumerable<UserProfileDto>> GetUsersAsync();

    IEnumerable<string> GetRoles();

    Task<UserProfileDto> AssignRoleAsync(string userId, string role);

    Task<UserProfileDto> RemoveRoleAsync(string userId, string role, string currentUserId);

    Task<UserProfileDto> LinkAsync(string userId, LinkUserDto dto);
}
