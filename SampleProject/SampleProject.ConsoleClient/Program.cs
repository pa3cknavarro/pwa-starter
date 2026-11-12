// Sample console application that consumes SampleProject via the typed client library.
using SampleProject.Client;
using SampleProject.Client.Models;
using SampleProject.Client.Services;
using Microsoft.Extensions.DependencyInjection;

// ---------------------------------------------------------------------------
// Configuration – change BaseUrl to match where SampleProject is running
// ---------------------------------------------------------------------------
const string ApiBaseUrl = "https://localhost:7xxx"; // <-- update port after `dotnet run` on the API
const string ApiKey = "demo-api-key-12345";

Console.WriteLine("=== SampleProject Console Client ===\n");
Console.WriteLine($"Target API: {ApiBaseUrl}");
Console.WriteLine("Make sure SampleProject is running first.\n");

// Build a minimal DI container and register the typed client
var services = new ServiceCollection();
services.AddSampleProjectClient(opts =>
{
    opts.BaseUrl = ApiBaseUrl;
    opts.ApiKey = ApiKey;
});
// Allow self-signed dev certificates
services.AddHttpClient("insecure")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// Re-register the typed client so it uses an insecure handler for local HTTPS
services.AddHttpClient<SampleProjectClient>(client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

await using var sp = services.BuildServiceProvider();
var api = sp.GetRequiredService<SampleProjectClient>();

try
{
    // 1) List provinces
    Console.WriteLine("--- List Provinces ---");
    var provinces = await api.GetProvincesAsync(new ListQuery { Page = 1, PageSize = 10, SortBy = "name" });
    if (!provinces.IsSuccess)
    {
        PrintError(provinces);
        return;
    }
    foreach (var p in provinces.Data!.Items)
        Console.WriteLine($"  {p.ProvinceId}  {p.ProvinceName}  (by {p.CreatedBy})");

    // 2) Create a province (with idempotency key)
    Console.WriteLine("\n--- Create Province ---");
    var createReq = new CreateProvinceRequest
    {
        ProvinceName = $"Demo Province {DateTime.UtcNow:HHmmss}",
        CreatedBy = "console-client"
    };
    var idemKey = Guid.NewGuid().ToString();
    var created = await api.CreateProvinceAsync(createReq, idemKey);
    if (!created.IsSuccess)
    {
        PrintError(created);
    }
    else
    {
        Console.WriteLine($"  Created: {created.Data!.ProvinceId} – {created.Data.ProvinceName}");
        Console.WriteLine($"  HATEOAS links: {string.Join(", ", created.Data.Links.Select(l => l.Rel))}");

        // 3) Get by id
        Console.WriteLine("\n--- Get Province by Id ---");
        var one = await api.GetProvinceAsync(created.Data.ProvinceId);
        if (one.IsSuccess)
            Console.WriteLine($"  {one.Data!.ProvinceName} created on {one.Data.CreatedOn:u}");

        // 4) List cities under first existing province (or the new one)
        var parentId = provinces.Data!.Items.FirstOrDefault()?.ProvinceId ?? created.Data.ProvinceId;
        Console.WriteLine($"\n--- List Cities (province {parentId}) ---");
        var cities = await api.GetCitiesAsync(new ListQuery { PageSize = 5 }, provinceId: parentId);
        if (cities.IsSuccess)
        {
            foreach (var c in cities.Data!.Items)
                Console.WriteLine($"  {c.CityName} ({c.ZipCode})");
        }

        // 5) Create a city under the new province
        Console.WriteLine("\n--- Create City ---");
        var cityReq = new CreateCityRequest
        {
            ProvinceId = created.Data.ProvinceId,
            CityName = "Demo City",
            ZipCode = "9999",
            CreatedBy = "console-client"
        };
        var city = await api.CreateCityAsync(cityReq, Guid.NewGuid().ToString());
        if (city.IsSuccess)
        {
            Console.WriteLine($"  Created city: {city.Data!.CityName} / {city.Data.CityId}");

            // 6) Create a barangay under that city
            Console.WriteLine("\n--- Create Barangay ---");
            var brgyReq = new CreateBarangayRequest
            {
                ProvinceId = city.Data.ProvinceId,
                CityId = city.Data.CityId,
                BarangayName = "Demo Barangay",
                CreatedBy = "console-client"
            };
            var brgy = await api.CreateBarangayAsync(brgyReq, Guid.NewGuid().ToString());
            if (brgy.IsSuccess)
                Console.WriteLine($"  Created barangay: {brgy.Data!.BarangayName} / {brgy.Data.BarangayId}");
            else
                PrintError(brgy);
        }
        else
        {
            PrintError(city);
        }

        // 7) Clean up – delete the province we created (cascade not configured, so may fail if children exist)
        Console.WriteLine("\n--- Cleanup (delete created province) ---");
        var del = await api.DeleteProvinceAsync(created.Data.ProvinceId);
        Console.WriteLine(del.IsSuccess
            ? "  Deleted."
            : $"  Delete status {del.StatusCode}: {del.Error?.Detail ?? "see children first"}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"\nConnection failed: {ex.Message}");
    Console.WriteLine("Is SampleProject running? Update ApiBaseUrl in Program.cs to the correct port.");
}
catch (Exception ex)
{
    Console.WriteLine($"\nUnexpected error: {ex.Message}");
}

Console.WriteLine("\nDone.");

static void PrintError<T>(ApiResult<T> result)
{
    Console.WriteLine($"  ERROR {result.StatusCode}: {result.Error?.Title} – {result.Error?.Detail}");
}
