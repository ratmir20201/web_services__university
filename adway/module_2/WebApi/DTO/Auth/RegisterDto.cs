using System.ComponentModel.DataAnnotations;

namespace WebApi.DTO.Auth;

public class RegisterDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "User name may contain only latin letters, digits, '.', '_' and '-'.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
