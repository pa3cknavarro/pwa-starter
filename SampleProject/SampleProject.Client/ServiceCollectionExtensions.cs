using SampleProject.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SampleProject.Client;

/// <summary>
/// Extension methods to register the typed Ecommerce API client with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers SampleProjectClient as a typed HttpClient with the given options.
    /// </summary>
    public static IServiceCollection AddSampleProjectClient(
        this IServiceCollection services,
        Action<SampleProjectOptions> configure)
    {
        var options = new SampleProjectOptions();
        configure(options);
        services.AddSingleton(options);

        services.AddHttpClient<SampleProjectClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    /// <summary>
    /// Registers SampleProjectClient using already-configured SampleProjectOptions from DI/config.
    /// </summary>
    public static IServiceCollection AddSampleProjectClient(this IServiceCollection services)
    {
        services.AddHttpClient<SampleProjectClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<SampleProjectOptions>();
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        return services;
    }
}
