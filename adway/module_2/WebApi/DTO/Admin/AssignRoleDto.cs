using System.ComponentModel.DataAnnotations;

namespace WebApi.DTO.Admin;

public class AssignRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
