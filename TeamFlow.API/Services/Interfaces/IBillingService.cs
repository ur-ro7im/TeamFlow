using TeamFlow.API.DTOs.Billing;
using TeamFlow.API.DTOs.Plans;

namespace TeamFlow.API.Services.Interfaces;

public interface IBillingService
{
    Task<List<PlanResponseDto>> GetPlansAsync();

    Task<SubscriptionResponseDto> GetCurrentSubscriptionAsync();

    Task<SubscriptionResponseDto> ChangePlanAsync(ChangePlanDto dto);

    Task<List<PaymentResponseDto>> GetPaymentsAsync();
}
