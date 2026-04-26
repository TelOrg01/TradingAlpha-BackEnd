using System.Net;
using System.Text.Json;

namespace TradingAlpha.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// 
/// Catches ALL unhandled exceptions and returns consistent JSON responses.
/// Must be registered FIRST in the middleware pipeline so it wraps everything.
/// 
/// ApplicationException → 400 Bad Request (business logic errors)
/// ArgumentException    → 400 Bad Request (validation errors)
/// UnauthorizedAccess   → 401 Unauthorized
/// Everything else      → 500 Internal Server Error
/// </summary>
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Determine status code and message based on exception type
        var (statusCode, message) = exception switch
        {
            ApplicationException appEx =>
                (HttpStatusCode.BadRequest, appEx.Message),

            ArgumentException argEx =>
                (HttpStatusCode.BadRequest, argEx.Message),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized access."),

            _ => (HttpStatusCode.InternalServerError,
                  "An unexpected error occurred. Please try again later.")
        };

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Business error: {Message}", exception.Message);
        }

        // Prevent writing if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started, cannot write error response.");
            return;
        }

        // Write JSON error response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            message = message
        });

        await context.Response.WriteAsync(response);
    }
}