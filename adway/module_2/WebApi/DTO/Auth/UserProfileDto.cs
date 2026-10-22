namespace WebApi.DTO.Auth;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    public int? StudentId { get; set; }

    public int? TeacherId { get; set; }
}
