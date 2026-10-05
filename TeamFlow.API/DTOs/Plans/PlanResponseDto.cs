namespace TeamFlow.API.DTOs.Plans;

public class PlanResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    // -1 means unlimited
    public int MaxMembers { get; set; }

    public int MaxProjects { get; set; }

    public long MaxStorage { get; set; }

    public bool IsFree { get; set; }
}
