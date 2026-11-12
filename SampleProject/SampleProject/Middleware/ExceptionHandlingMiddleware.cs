// Namespace for middleware
namespace SampleProject.Middleware;

// Import shared error type
using SampleProject.Common;

/// <summary>
/// Global exception handler that converts unhandled exceptions into a consistent ErrorEnvelope.
/// Guarantees Meaningful Status Codes and Error Envelopes.
/// </summary>
public class ExceptionHandlingMiddleware
{
    // Next middleware
    private readonly RequestDelegate _next;
    // Logger for recording unexpected failures
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // Constructor
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Wraps the rest of the pipeline in a try/catch and maps exceptions to HTTP responses.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the full exception for diagnostics
            _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);

            // Map known exception types to appropriate status codes
            var (status, title) = ex switch
            {
                ArgumentException or ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            // Build the standard error envelope
            var error = new ErrorEnvelope
            {
                TraceId = context.TraceIdentifier,
                Status = status,
                Title = title,
                Detail = ex.Message
            };

            // Write the error response
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(error);
        }
    }
}
