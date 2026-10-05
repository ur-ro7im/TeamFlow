using TeamFlow.API.DTOs.Plans;

namespace TeamFlow.API.DTOs.Billing;

public class SubscriptionResponseDto
{
    public PlanResponseDto Plan { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public UsageDto Usage { get; set; } = null!;
}

public class UsageDto
{
    public int Members { get; set; }

    public int Projects { get; set; }

    public long StorageBytes { get; set; }
}
