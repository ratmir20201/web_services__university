using WebApi.DTO.Auth;
using WebApi.Models;

namespace WebApi.Services.Interfaces;

public interface ITokenService
{
    AuthResponseDto CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles);
}
