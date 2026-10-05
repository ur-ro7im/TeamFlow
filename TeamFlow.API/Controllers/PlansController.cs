using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlansController : ControllerBase
{
    private readonly IBillingService _billingService;

    public PlansController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    // Public: the sign-up screen shows the plans before an account exists.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans()
    {
        return Ok(await _billingService.GetPlansAsync());
    }
}
