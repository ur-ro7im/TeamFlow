using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Auth;

public class LoginDto
{
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = null!;
}
