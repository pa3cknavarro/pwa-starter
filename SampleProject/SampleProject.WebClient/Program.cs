using SampleProject.Client;
using SampleProject.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages UI
builder.Services.AddRazorPages();

// Bind Ecommerce API options from configuration section "SampleProject"
builder.Services.Configure<SampleProjectOptions>(
    builder.Configuration.GetSection("SampleProject"));

// Register a concrete options instance for the typed client
var apiOptions = new SampleProjectOptions();
builder.Configuration.GetSection("SampleProject").Bind(apiOptions);
builder.Services.AddSingleton(apiOptions);

// Typed HttpClient that calls SampleProject
// In Development we accept the self-signed HTTPS certificate used by the API
builder.Services.AddHttpClient<SampleProjectClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<SampleProjectOptions>();
    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("X-Api-Key", opts.ApiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // Allow self-signed certs when calling local SampleProject over HTTPS
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
