using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Billing;

/// <summary>
/// Card details for the SIMULATED payment gateway only. They are validated in memory and
/// never stored or logged. A real gateway (Stripe, Paymob, ...) must be integrated with
/// hosted fields / tokens so raw card numbers never reach this API.
/// </summary>
public class MockCardDto
{
    [Required]
    [MaxLength(100)]
    public string CardHolder { get; set; } = null!;

    [Required]
    [MaxLength(25)]
    public string CardNumber { get; set; } = null!;

    [Range(1, 12)]
    public int ExpiryMonth { get; set; }

    [Range(2000, 2100)]
    public int ExpiryYear { get; set; }

    [Required]
    [MaxLength(4)]
    public string Cvc { get; set; } = null!;
}
