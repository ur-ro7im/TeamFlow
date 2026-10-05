using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamFlow.API.DTOs.Billing;
using TeamFlow.API.Services.Interfaces;

namespace TeamFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription()
    {
        return Ok(await _billingService.GetCurrentSubscriptionAsync());
    }

    [HttpGet("payments")]
    [Authorize(Policy = "OwnerOnly")]
    public async Task<IActionResult> GetPayments()
    {
        return Ok(await _billingService.GetPaymentsAsync());
    }

    [HttpPost("change-plan")]
    [Authorize(Policy = "OwnerOnly")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ChangePlan(ChangePlanDto dto)
    {
        return Ok(await _billingService.ChangePlanAsync(dto));
    }
}
