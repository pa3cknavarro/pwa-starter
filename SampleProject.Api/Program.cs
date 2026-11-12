// Entry point of the Ecommerce API application.
// Configures services and the HTTP request pipeline for a Vertical Slice + Carter architecture.

using System.Threading.RateLimiting; // Required for RateLimiterOptions and FixedWindowRateLimiter
using Asp.Versioning;               // API versioning abstractions
using Carter;                       // Carter module registration
using Ecommerce.Api.Common;         // Shared constants and helpers
using Ecommerce.Api.Data;           // AppDbContext
using Ecommerce.Api.Middleware;     // Custom middleware
using Microsoft.EntityFrameworkCore; // EF Core registration
using Microsoft.OpenApi.Models;     // Swagger / OpenAPI models

// Create the WebApplication builder – the starting point for configuration
var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------------------------
// SERVICE REGISTRATION
// --------------------------------------------------------------------------

// Register the in-memory EF Core database context (demo only; replace with real provider in production)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("EcommerceDb")); // Database name is arbitrary for InMemory

// Register the singleton IdempotencyStore so the middleware can share state across requests
builder.Services.AddSingleton<IdempotencyStore>();

// Add Carter – discovers all ICarterModule implementations and registers their routes
builder.Services.AddCarter();

// Configure API Versioning (Versioning requirement)
builder.Services.AddApiVersioning(options =>
{
    // Default version when the client does not specify one
    options.DefaultApiVersion = new ApiVersion(1, 0);
    // Assume the default version when none is supplied
    options.AssumeDefaultVersionWhenUnspecified = true;
    // Report supported versions in response headers (api-supported-versions, api-deprecated-versions)
    options.ReportApiVersions = true;
    // Read version from URL segment (/api/v1/...), query string, or header
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));
})
.AddApiExplorer(options =>
{
    // Format used by the API explorer / Swagger (v'VVV' produces "v1", "v2", …)
    options.GroupNameFormat = "'v'VVV";
    // Substitute the version into the URL template automatically
    options.SubstituteApiVersionInUrl = true;
});

// Register Rate Limiting (Rate Limiting requirement)
builder.Services.AddRateLimiter(options =>
{
    // Global limiter that applies to all endpoints unless overridden
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Partition key – here we use the client IP; could also use API key
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                // Allow 100 requests
                PermitLimit = 100,
                // Per 1-minute window
                Window = TimeSpan.FromMinutes(1),
                // Queue excess requests instead of rejecting immediately
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    // Custom rejection response that still follows the ErrorEnvelope shape
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new ErrorEnvelope
        {
            TraceId = context.HttpContext.TraceIdentifier,
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Detail = "Rate limit exceeded. Please try again later.",
            Extensions = new Dictionary<string, object>
            {
                ["retryAfterSeconds"] = 60
            }
        }, cancellationToken);
    };
});

// Configure JSON serialization for consistent camelCase naming (Consistent Naming)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true; // Nice for demos; disable in production if desired
});

// Add OpenAPI / Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Document for version 1.0
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce Location API",
        Version = "v1",
        Description = "Vertical-slice API demonstrating Versioning, Pagination, Rate Limiting, Idempotency, HATEOAS, etc."
    });

    // Tell Swagger how to send the API key header
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key authentication via the X-Api-Key header",
        Name = ApiConstants.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

// --------------------------------------------------------------------------
// BUILD THE APPLICATION
// --------------------------------------------------------------------------
var app = builder.Build();

// --------------------------------------------------------------------------
// SEED DEMO DATA (so the API is immediately usable)
// --------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Only seed if the database is empty
    if (!db.Provinces.Any())
    {
        var ncr = new Ecommerce.Api.Models.Province
        {
            ProvinceId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ProvinceName = "Metro Manila",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow
        };
        var cebu = new Ecommerce.Api.Models.Province
        {
            ProvinceId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ProvinceName = "Cebu",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow
        };
        db.Provinces.AddRange(ncr, cebu);

        var makati = new Ecommerce.Api.Models.City
        {
            ProvinceId = ncr.ProvinceId,
            CityId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            CityName = "Makati",
            ZipCode = "1200",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow
        };
        db.Cities.Add(makati);

        db.Barangays.Add(new Ecommerce.Api.Models.Barangay
        {
            ProvinceId = ncr.ProvinceId,
            CityId = makati.CityId,
            BarangayId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            BarangayName = "Poblacion",
            CreatedBy = "seed",
            CreatedOn = DateTime.UtcNow
        });

        db.SaveChanges();
    }
}

// --------------------------------------------------------------------------
// HTTP PIPELINE CONFIGURATION (order matters)
// --------------------------------------------------------------------------

// Global exception handler – must be early so it catches everything
app.UseMiddleware<ExceptionHandlingMiddleware>();

// HTTPS redirection (good practice)
app.UseHttpsRedirection();

// Enable rate limiting middleware
app.UseRateLimiter();

// Custom API-key authentication middleware (Auth in Headers)
app.UseMiddleware<ApiKeyAuthMiddleware>();

// Idempotency-Key middleware – must be after auth but before endpoints
app.UseMiddleware<IdempotencyMiddleware>();

// Swagger UI only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
    });
}

// Map all Carter modules (the vertical slices)
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapCarter();

// --------------------------------------------------------------------------
// RUN THE APPLICATION
// --------------------------------------------------------------------------
app.Run();
