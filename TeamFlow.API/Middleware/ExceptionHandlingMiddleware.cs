using System.Net;
using TeamFlow.API.Exceptions;

namespace TeamFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected; nothing to send back.
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ConflictException => (HttpStatusCode.Conflict, exception.Message),
            PaymentRequiredException => (HttpStatusCode.PaymentRequired, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            // Full details go to the log only; the client never sees stack traces or internals.
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            statusCode = (int)statusCode,
            message
        });
    }
}
