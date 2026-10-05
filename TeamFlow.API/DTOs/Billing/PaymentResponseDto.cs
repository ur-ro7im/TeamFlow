namespace TeamFlow.API.DTOs.Billing;

public class PaymentResponseDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; }
}
