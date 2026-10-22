namespace WebApi.DTO.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }

    public string UserName { get; set; } = string.Empty;

    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}
