using TeamFlow.API.Models.Enums;

namespace TeamFlow.API.Models;

public class Subscription
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public int PlanId { get; set; }

    public SubscriptionStatus Status { get; set; } 

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }


    // Navigation Properties

    public Organization Organization { get; set; } = null!;

    public Plan Plan { get; set; } = null!;

    public ICollection<Payment> Payments
    {
        get;
        set;
    } = new List<Payment>();
}