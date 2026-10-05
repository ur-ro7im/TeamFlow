namespace TeamFlow.API.Exceptions;

public class PaymentRequiredException : Exception
{
    public PaymentRequiredException(string message)
        : base(message)
    {
    }
}
