// Namespace for shared constants
namespace Ecommerce.Api.Common;

/// <summary>
/// Central place for magic strings used by the API (headers, policies, etc.).
/// </summary>
public static class ApiConstants
{
    // Header name that clients must send for authentication (Auth in Headers requirement)
    public const string ApiKeyHeaderName = "X-Api-Key";

    // Expected value of the API key for this demo (in real apps use configuration + secrets)
    public const string ExpectedApiKey = "demo-api-key-12345";

    // Header name used for idempotent write operations
    public const string IdempotencyKeyHeaderName = "X-Idempotency-Key";

    // Rate limiting policy name registered in Program.cs
    public const string RateLimitPolicyName = "fixed";
}
