// Namespace for custom middleware components
namespace Ecommerce.Api.Middleware;

// Import the constants that define header names and expected values
using Ecommerce.Api.Common;

/// <summary>
/// Middleware that enforces presence of a valid API key in the request headers.
/// Implements the "Auth in Headers" requirement.
/// Sample pages and Swagger are excluded so the demo UI can load without a key.
/// </summary>
public class ApiKeyAuthMiddleware
{
    // Next middleware in the pipeline; invoked when authentication succeeds
    private readonly RequestDelegate _next;

    // Constructor receives the next delegate via dependency injection
    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        // Store the next middleware so we can call it later
        _next = next;
    }

    /// <summary>
    /// Invoked for every request. Checks the X-Api-Key header and either continues or short-circuits.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for Swagger, OpenAPI docs, and the sample HTML/JS pages
        var path = context.Request.Path.Value ?? string.Empty;
        var isPublic =
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/samples", StringComparison.OrdinalIgnoreCase) ||
            path == "/" ||
            path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".css", StringComparison.OrdinalIgnoreCase);

        if (isPublic)
        {
            // Allow the request to proceed without an API key
            await _next(context);
            return;
        }

        // Try to read the API key header; if missing or wrong, return 401
        if (!context.Request.Headers.TryGetValue(ApiConstants.ApiKeyHeaderName, out var extractedApiKey) ||
            extractedApiKey != ApiConstants.ExpectedApiKey)
        {
            // Build a standard error envelope for the unauthorized response
            var error = new ErrorEnvelope
            {
                TraceId = context.TraceIdentifier,
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = $"A valid '{ApiConstants.ApiKeyHeaderName}' header is required."
            };

            // Set the HTTP status code to 401
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            // Write the error envelope as JSON
            await context.Response.WriteAsJsonAsync(error);
            // Do not call the next middleware
            return;
        }

        // API key is valid – continue the pipeline
        await _next(context);
    }
}
