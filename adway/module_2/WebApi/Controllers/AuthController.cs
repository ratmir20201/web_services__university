using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Auth;
using WebApi.Common;
using WebApi.Common.Exceptions;
using WebApi.DTO.Auth;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        _logger.LogInformation(
            "Получен запрос на регистрацию пользователя {UserName}",
            dto.UserName);

        var profile = await _authService.RegisterAsync(dto);

        _logger.LogInformation(
            "Пользователь {UserName} успешно зарегистрирован",
            profile.UserName);

        return StatusCode(
            201,
            new ReturnResult<UserProfileDto>
            {
                StatusCode = 201,
                IsSuccess = true,
                Result = profile,
                TraceId = HttpContext.TraceIdentifier
            });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        _logger.LogInformation(
            "Попытка входа пользователя {UserName}",
            dto.UserName);

        var token = await _authService.LoginAsync(dto);

        _logger.LogInformation(
            "Пользователь {UserName} успешно вошел в систему",
            token.UserName);

        return Ok(new ReturnResult<AuthResponseDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = token,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId()
            ?? throw new ApiException(401, "UNAUTHORIZED", "Authentication is required.");

        var profile = await _authService.GetProfileAsync(userId);

        return Ok(new ReturnResult<UserProfileDto>
        {
            StatusCode = 200,
            IsSuccess = true,
            Result = profile,
            TraceId = HttpContext.TraceIdentifier
        });
    }
}
