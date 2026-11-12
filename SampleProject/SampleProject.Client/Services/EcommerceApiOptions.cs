namespace SampleProject.Client.Services;

/// <summary>
/// Configuration for the typed Ecommerce API client.
/// Bind from appsettings or set in code.
/// </summary>
public class SampleProjectOptions
{
    /// <summary>Base URL of the SampleProject host, e.g. https://localhost:7xxx</summary>
    public string BaseUrl { get; set; } = "https://localhost:5001";

    /// <summary>Value for the X-Api-Key header required by the API.</summary>
    public string ApiKey { get; set; } = "demo-api-key-12345";

    /// <summary>Optional default API version (URL segment uses v1 by default in paths).</summary>
    public string ApiVersion { get; set; } = "v1";
}
