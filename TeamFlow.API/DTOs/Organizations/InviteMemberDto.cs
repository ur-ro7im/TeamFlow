using System.ComponentModel.DataAnnotations;

public class InviteMemberDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;
}
