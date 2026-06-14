using System.ComponentModel.DataAnnotations;

namespace DoubleCheck.Dtos;

public class AssignRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
