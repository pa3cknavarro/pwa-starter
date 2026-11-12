// Namespace for the Cities vertical slice
namespace Ecommerce.Api.Features.Cities;

// Carter module base
using Carter;
// EF Core for database access
using Microsoft.EntityFrameworkCore;
// Shared helpers (pagination, error envelope, HATEOAS)
using Ecommerce.Api.Common;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
// API versioning
using Asp.Versioning;

/// <summary>
/// Carter module that registers all endpoints for the Cities feature.
/// Demonstrates the same cross-cutting concerns as the Province slice:
/// Versioning, Pagination, Filtering + Sorting, HATEOAS, Meaningful Status Codes,
/// Idempotency (via middleware), Auth in Headers, Error Envelopes.
/// </summary>
public class CityEndpoints : ICarterModule
{
    // Called by Carter at startup to map the routes for this module
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Versioned route group: /api/v{version}/cities
        var group = app.NewVersionedApi()
            .MapGroup("/api/v{version:apiVersion}/cities")
            .HasApiVersion(1.0)
            .WithTags("Cities");

        // GET /api/v1/cities – list with optional province filter, pagination, sorting
        group.MapGet("/", GetCitiesAsync)
            .WithName("GetCities")
            .Produces<PaginatedResponse<CityResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest);

        // GET /api/v1/cities/{provinceId}/{cityId} – single city by composite key
        group.MapGet("/{provinceId:guid}/{cityId:guid}", GetCityByIdAsync)
            .WithName("GetCityById")
            .Produces<CityResponse>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);

        // POST /api/v1/cities – create a new city (Idempotency-Key supported via middleware)
        group.MapPost("/", CreateCityAsync)
            .WithName("CreateCity")
            .Produces<CityResponse>(StatusCodes.Status201Created)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest)
            .Produces<ErrorEnvelope>(StatusCodes.Status409Conflict);

        // PUT /api/v1/cities/{provinceId}/{cityId} – full update
        group.MapPut("/{provinceId:guid}/{cityId:guid}", UpdateCityAsync)
            .WithName("UpdateCity")
            .Produces<CityResponse>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest);

        // DELETE /api/v1/cities/{provinceId}/{cityId}
        group.MapDelete("/{provinceId:guid}/{cityId:guid}", DeleteCityAsync)
            .WithName("DeleteCity")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Handler for GET /cities – returns a paginated, filterable, sortable list of cities.
    /// Optional query parameter: provinceId (filters to a single province).
    /// </summary>
    private static async Task<IResult> GetCitiesAsync(
        [AsParameters] PaginationParams query, // page, pageSize, search, sortBy, sortOrder
        Guid? provinceId,                     // optional parent filter
        AppDbContext db,
        HttpContext http)
    {
        // Clamp page size to a safe range
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        // Start with all cities (no tracking for read performance)
        IQueryable<City> source = db.Cities.AsNoTracking();

        // Filter by parent province when supplied
        if (provinceId.HasValue)
            source = source.Where(c => c.ProvinceId == provinceId.Value);

        // Free-text search on city name
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(c => c.CityName.ToLower().Contains(term));
        }

        // Apply sorting
        source = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
        {
            ("name", "desc") => source.OrderByDescending(c => c.CityName),
            ("name", _) => source.OrderBy(c => c.CityName),
            ("zipcode", "desc") => source.OrderByDescending(c => c.ZipCode),
            ("zipcode", _) => source.OrderBy(c => c.ZipCode),
            ("createdon", "desc") => source.OrderByDescending(c => c.CreatedOn),
            ("createdon", _) => source.OrderBy(c => c.CreatedOn),
            _ => source.OrderBy(c => c.CityName)
        };

        // Total count before paging (needed for pagination metadata)
        var total = await source.CountAsync();

        // Fetch the requested page and project to the response DTO
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CityResponse
            {
                ProvinceId = c.ProvinceId,
                CityId = c.CityId,
                CityName = c.CityName,
                ZipCode = c.ZipCode,
                CreatedBy = c.CreatedBy,
                CreatedOn = c.CreatedOn
            })
            .ToListAsync();

        // Build absolute base URL for HATEOAS links
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";

        // Attach per-item links
        foreach (var item in items)
        {
            item.Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/cities/{item.ProvinceId}/{item.CityId}", Method = "GET" },
                new HateoasLink { Rel = "update", Href = $"{baseUrl}/api/v1/cities/{item.ProvinceId}/{item.CityId}", Method = "PUT" },
                new HateoasLink { Rel = "delete", Href = $"{baseUrl}/api/v1/cities/{item.ProvinceId}/{item.CityId}", Method = "DELETE" },
                new HateoasLink { Rel = "province", Href = $"{baseUrl}/api/v1/provinces/{item.ProvinceId}", Method = "GET" },
                new HateoasLink { Rel = "barangays", Href = $"{baseUrl}/api/v1/barangays?provinceId={item.ProvinceId}&cityId={item.CityId}", Method = "GET" }
            ];
        }

        // Build the paginated response envelope
        var response = new PaginatedResponse<CityResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        // Collection-level HATEOAS links
        response.Links.Add(new HateoasLink
        {
            Rel = "self",
            Href = $"{baseUrl}/api/v1/cities?page={page}&pageSize={pageSize}",
            Method = "GET"
        });
        if (page * pageSize < total)
        {
            response.Links.Add(new HateoasLink
            {
                Rel = "next",
                Href = $"{baseUrl}/api/v1/cities?page={page + 1}&pageSize={pageSize}",
                Method = "GET"
            });
        }
        if (page > 1)
        {
            response.Links.Add(new HateoasLink
            {
                Rel = "prev",
                Href = $"{baseUrl}/api/v1/cities?page={page - 1}&pageSize={pageSize}",
                Method = "GET"
            });
        }

        // 200 OK with the paginated payload
        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for GET /cities/{provinceId}/{cityId}
    /// </summary>
    private static async Task<IResult> GetCityByIdAsync(
        Guid provinceId,
        Guid cityId,
        AppDbContext db,
        HttpContext http)
    {
        var city = await db.Cities.AsNoTracking()
            .FirstOrDefaultAsync(c => c.ProvinceId == provinceId && c.CityId == cityId);

        // 404 with standard error envelope when not found
        if (city is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"City with ProvinceId '{provinceId}' and CityId '{cityId}' was not found."
            });
        }

        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new CityResponse
        {
            ProvinceId = city.ProvinceId,
            CityId = city.CityId,
            CityName = city.CityName,
            ZipCode = city.ZipCode,
            CreatedBy = city.CreatedBy,
            CreatedOn = city.CreatedOn,
            Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/cities/{city.ProvinceId}/{city.CityId}", Method = "GET" },
                new HateoasLink { Rel = "update", Href = $"{baseUrl}/api/v1/cities/{city.ProvinceId}/{city.CityId}", Method = "PUT" },
                new HateoasLink { Rel = "delete", Href = $"{baseUrl}/api/v1/cities/{city.ProvinceId}/{city.CityId}", Method = "DELETE" },
                new HateoasLink { Rel = "province", Href = $"{baseUrl}/api/v1/provinces/{city.ProvinceId}", Method = "GET" },
                new HateoasLink { Rel = "barangays", Href = $"{baseUrl}/api/v1/barangays?provinceId={city.ProvinceId}&cityId={city.CityId}", Method = "GET" }
            ]
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for POST /cities – creates a new city under an existing province.
    /// </summary>
    private static async Task<IResult> CreateCityAsync(
        CreateCityRequest request,
        AppDbContext db,
        HttpContext http)
    {
        // Validation – return 400 with field-level errors when name is missing
        if (string.IsNullOrWhiteSpace(request.CityName))
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "CityName is required.",
                Errors = new Dictionary<string, string[]>
                {
                    ["CityName"] = ["The CityName field is required."]
                }
            });
        }

        // Ensure the parent province exists
        var provinceExists = await db.Provinces.AnyAsync(p => p.ProvinceId == request.ProvinceId);
        if (!provinceExists)
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = $"Province with id '{request.ProvinceId}' does not exist."
            });
        }

        // Optional uniqueness check (same name under the same province)
        var duplicate = await db.Cities.AnyAsync(c =>
            c.ProvinceId == request.ProvinceId &&
            c.CityName == request.CityName.Trim());
        if (duplicate)
        {
            return Results.Conflict(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = $"A city named '{request.CityName}' already exists in this province."
            });
        }

        // Create the entity
        var city = new City
        {
            ProvinceId = request.ProvinceId,
            CityId = Guid.NewGuid(),
            CityName = request.CityName.Trim(),
            ZipCode = request.ZipCode?.Trim() ?? string.Empty,
            CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "system" : request.CreatedBy!,
            CreatedOn = DateTime.UtcNow
        };

        db.Cities.Add(city);
        await db.SaveChangesAsync();

        // Build response with HATEOAS and Location header
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new CityResponse
        {
            ProvinceId = city.ProvinceId,
            CityId = city.CityId,
            CityName = city.CityName,
            ZipCode = city.ZipCode,
            CreatedBy = city.CreatedBy,
            CreatedOn = city.CreatedOn,
            Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/cities/{city.ProvinceId}/{city.CityId}", Method = "GET" }
            ]
        };

        // 201 Created
        return Results.Created($"/api/v1/cities/{city.ProvinceId}/{city.CityId}", response);
    }

    /// <summary>
    /// Handler for PUT /cities/{provinceId}/{cityId}
    /// </summary>
    private static async Task<IResult> UpdateCityAsync(
        Guid provinceId,
        Guid cityId,
        UpdateCityRequest request,
        AppDbContext db,
        HttpContext http)
    {
        var city = await db.Cities.FirstOrDefaultAsync(c => c.ProvinceId == provinceId && c.CityId == cityId);
        if (city is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"City with ProvinceId '{provinceId}' and CityId '{cityId}' was not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.CityName))
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "CityName is required."
            });
        }

        // Apply changes
        city.CityName = request.CityName.Trim();
        city.ZipCode = request.ZipCode?.Trim() ?? string.Empty;
        await db.SaveChangesAsync();

        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new CityResponse
        {
            ProvinceId = city.ProvinceId,
            CityId = city.CityId,
            CityName = city.CityName,
            ZipCode = city.ZipCode,
            CreatedBy = city.CreatedBy,
            CreatedOn = city.CreatedOn,
            Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/cities/{city.ProvinceId}/{city.CityId}", Method = "GET" }
            ]
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for DELETE /cities/{provinceId}/{cityId}
    /// </summary>
    private static async Task<IResult> DeleteCityAsync(
        Guid provinceId,
        Guid cityId,
        AppDbContext db,
        HttpContext http)
    {
        var city = await db.Cities.FirstOrDefaultAsync(c => c.ProvinceId == provinceId && c.CityId == cityId);
        if (city is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"City with ProvinceId '{provinceId}' and CityId '{cityId}' was not found."
            });
        }

        db.Cities.Remove(city);
        await db.SaveChangesAsync();

        // 204 No Content – successful deletion with empty body
        return Results.NoContent();
    }
}
