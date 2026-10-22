using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Auth;
using WebApi.DTO.Auth;
using WebApi.Models;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AuthResponseDto CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(AppClaimTypes.UserId, user.Id),
            new(AppClaimTypes.UserName, user.UserName ?? string.Empty),
            new(AppClaimTypes.TokenId, Guid.NewGuid().ToString())
        };

        claims.AddRange(roleList.Select(role => new Claim(AppClaimTypes.Role, role)));

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.StudentId, user.StudentId.Value.ToString()));
        }

        if (user.TeacherId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.TeacherId, user.TeacherId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AuthResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            UserName = user.UserName ?? string.Empty,
            Roles = roleList
        };
    }
}
