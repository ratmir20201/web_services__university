using WebApi.DTO.Auth;

namespace WebApi.Services.Interfaces;

public interface IAuthService
{
    Task<UserProfileDto> RegisterAsync(RegisterDto dto);

    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    Task<UserProfileDto> GetProfileAsync(string userId);
}
