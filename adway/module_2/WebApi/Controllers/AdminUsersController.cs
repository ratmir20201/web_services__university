using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Auth;
using WebApi.Common;
using WebApi.DTO.Admin;
using WebApi.DTO.Auth;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/admin")]
[Authorize(Policy = Policies.AdminOnly)]
public class AdminUsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IUserAdminService userAdminService,
        ILogger<AdminUsersController> logger)
    {
        _userAdminService = userAdminService;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userAdminService.GetUsersAsync();

        return Ok(new ReturnResult<IEnumerable<UserProfileDto>>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = users,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        return Ok(new ReturnResult<IEnumerable<string>>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = _userAdminService.GetRoles(),
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AssignRole(
        string userId,
        AssignRoleDto dto)
    {
        _logger.LogInformation(
            "Администратор {Admin} назначает роль {Role} пользователю {UserId}",
            User.Identity?.Name,
            dto.Role,
            userId);

        var profile = await _userAdminService.AssignRoleAsync(userId, dto.Role);

        return Ok(new ReturnResult<UserProfileDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = profile,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpDelete("users/{userId}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(
        string userId,
        string role)
    {
        _logger.LogInformation(
            "Администратор {Admin} снимает роль {Role} с пользователя {UserId}",
            User.Identity?.Name,
            role,
            userId);

        var profile = await _userAdminService.RemoveRoleAsync(
            userId,
            role,
            User.GetUserId() ?? string.Empty);

        return Ok(new ReturnResult<UserProfileDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = profile,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpPut("users/{userId}/links")]
    public async Task<IActionResult> Link(
        string userId,
        LinkUserDto dto)
    {
        _logger.LogInformation(
            "Администратор {Admin} связывает пользователя {UserId} со студентом {StudentId} и преподавателем {TeacherId}",
            User.Identity?.Name,
            userId,
            dto.StudentId,
            dto.TeacherId);

        var profile = await _userAdminService.LinkAsync(userId, dto);

        return Ok(new ReturnResult<UserProfileDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = profile,
            TraceId = HttpContext.TraceIdentifier
        });
    }
}
