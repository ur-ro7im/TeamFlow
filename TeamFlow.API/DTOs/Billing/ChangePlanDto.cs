using System.ComponentModel.DataAnnotations;

namespace TeamFlow.API.DTOs.Billing;

public class ChangePlanDto
{
    [Range(1, int.MaxValue)]
    public int PlanId { get; set; }

    // Required only when the target plan is not free.
    public MockCardDto? Card { get; set; }
}
