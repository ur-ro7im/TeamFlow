using TeamFlow.API.Exceptions;

namespace TeamFlow.API.Services.Payments;

/// <summary>
/// Simulated gateway for development and demos. No money moves.
/// Test cards:
///   4242 4242 4242 4242 -> approved
///   4000 0000 0000 0002 -> declined
///   4000 0000 0000 9995 -> declined (insufficient funds)
/// Any other card that passes the Luhn check is approved.
/// Card data is validated in memory only and is never stored or logged.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentGatewayResult> ChargeAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        var card = request.Card;

        var number = new string(
            card.CardNumber.Where(char.IsDigit).ToArray());

        if (number.Length < 13 || number.Length > 19 || !PassesLuhn(number))
        {
            throw new BadRequestException("The card number is not valid.");
        }

        var now = DateTime.UtcNow;

        var expiresAt = new DateTime(
            card.ExpiryYear,
            card.ExpiryMonth,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc).AddMonths(1);

        if (expiresAt <= now)
        {
            throw new BadRequestException("The card has expired.");
        }

        if (card.Cvc.Length < 3 || !card.Cvc.All(char.IsDigit))
        {
            throw new BadRequestException("The security code is not valid.");
        }

        var transactionId = $"mock_{Guid.NewGuid():N}";

        PaymentGatewayResult result = number switch
        {
            "4000000000000002" => new PaymentGatewayResult(
                false, transactionId, "The card was declined."),

            "4000000000009995" => new PaymentGatewayResult(
                false, transactionId, "The card has insufficient funds."),

            _ => new PaymentGatewayResult(true, transactionId, null)
        };

        _logger.LogInformation(
            "Mock payment {TransactionId} for {Amount} {Currency}: {Outcome}",
            transactionId,
            request.Amount,
            request.Currency,
            result.Succeeded ? "approved" : "declined");

        return Task.FromResult(result);
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var alternate = false;

        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';

            if (alternate)
            {
                d *= 2;

                if (d > 9)
                {
                    d -= 9;
                }
            }

            sum += d;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }
}
