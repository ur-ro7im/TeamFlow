using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [MaxLength(72)]
    public string Password { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string OrganizationName { get; set; } = null!;

    // Plan chosen on the sign-up screen. Omitted means the Free plan.
    public int? PlanId { get; set; }
}
