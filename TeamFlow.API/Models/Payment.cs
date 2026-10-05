using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Models;

public class Payment
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public PaymentStatus Status { get; set; } 

    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; }


    // Navigation Properties

    public Organization Organization { get; set; } = null!;

    public Subscription Subscription { get; set; } = null!;
}