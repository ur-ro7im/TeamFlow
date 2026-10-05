using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Auth;

public class RegisterFromInvitationDto
{
    [Required]
    [MaxLength(200)]
    public string Token { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    // BCrypt only uses the first 72 bytes of a password.
    [Required]
    [MinLength(8)]
    [MaxLength(72)]
    public string Password { get; set; } = null!;
}
