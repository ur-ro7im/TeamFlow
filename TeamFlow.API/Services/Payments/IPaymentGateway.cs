using TeamFlow.API.DTOs.Billing;

namespace TeamFlow.API.Services.Payments;

public record PaymentGatewayRequest(
    decimal Amount,
    string Currency,
    string Description,
    MockCardDto Card);

public record PaymentGatewayResult(
    bool Succeeded,
    string TransactionId,
    string? FailureReason);

/// <summary>
/// Abstraction over the payment provider. BillingService only depends on this interface,
/// so replacing <see cref="MockPaymentGateway"/> with a real provider is a one-class change.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default);
}
