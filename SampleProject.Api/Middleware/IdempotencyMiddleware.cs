// Namespace for custom middleware
namespace Ecommerce.Api.Middleware;

// Import shared types
using Ecommerce.Api.Common;
// Import System.Text for encoding
using System.Text;

/// <summary>
/// Middleware that implements Idempotency-Key support for non-safe HTTP methods.
/// If a matching key has already been processed, the original response is replayed.
/// </summary>
public class IdempotencyMiddleware
{
    // Next middleware in the pipeline
    private readonly RequestDelegate _next;

    // Constructor
    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Checks for X-Idempotency-Key on write operations and either replays or stores the response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IdempotencyStore store)
    {
        // Only apply to methods that change state (POST, PUT, PATCH, DELETE)
        if (HttpMethods.IsGet(context.Request.Method) ||
            HttpMethods.IsHead(context.Request.Method) ||
            HttpMethods.IsOptions(context.Request.Method))
        {
            // Safe methods – just continue
            await _next(context);
            return;
        }

        // Look for the Idempotency-Key header
        if (!context.Request.Headers.TryGetValue(ApiConstants.IdempotencyKeyHeaderName, out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.FirstOrDefault()))
        {
            // Header missing – still allow the request (or you could make it mandatory)
            await _next(context);
            return;
        }

        var key = keyValues.First()!;

        // If we already processed this key, replay the stored response
        if (store.TryGet(key, out var entry) && entry is not null)
        {
            context.Response.StatusCode = entry.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(entry.ResponseBody);
            return;
        }

        // Capture the original response body stream so we can read what was written
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        // Continue the pipeline – the endpoint will write into memStream
        await _next(context);

        // After the endpoint has finished, read the body we captured
        memStream.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(memStream).ReadToEndAsync();

        // Only store successful (2xx) responses so retries can still fail if needed
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            store.Set(key, context.Response.StatusCode, bodyText);
        }

        // Copy the captured body back to the original stream so the client receives it
        memStream.Seek(0, SeekOrigin.Begin);
        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
}
