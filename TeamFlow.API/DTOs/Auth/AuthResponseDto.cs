namespace TeamFlow.API.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int OrganizationId { get; set; }

    public string Role { get; set; } = null!;

    // The plan picked at registration; the account itself always starts on Free
    // and moves to a paid plan only after a successful payment.
    public int? SelectedPlanId { get; set; }

    public bool RequiresPayment { get; set; }
}
