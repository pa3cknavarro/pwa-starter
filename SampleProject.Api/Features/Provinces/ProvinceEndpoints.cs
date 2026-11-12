// Namespace for the Provinces feature slice
namespace Ecommerce.Api.Features.Provinces;

// Carter module base class
using Carter;
// EF Core for database access
using Microsoft.EntityFrameworkCore;
// Shared helpers
using Ecommerce.Api.Common;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;
// Versioning attributes
using Asp.Versioning;

/// <summary>
/// Carter module that registers all endpoints belonging to the Provinces vertical slice.
/// Demonstrates Versioning, Pagination, Filtering + Sorting, HATEOAS, Meaningful Status Codes.
/// </summary>
public class ProvinceEndpoints : ICarterModule
{
    // Called by Carter at startup to map routes for this module
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Create a versioned route group: /api/v{version}/provinces
        var group = app.NewVersionedApi()
            .MapGroup("/api/v{version:apiVersion}/provinces")
            .HasApiVersion(1.0)
            .HasApiVersion(2.0) // Example of supporting multiple versions
            .WithTags("Provinces");

        // GET /api/v1/provinces – list with pagination, filtering and sorting
        group.MapGet("/", GetProvincesAsync)
            .WithName("GetProvinces")
            .Produces<PaginatedResponse<ProvinceResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest);

        // GET /api/v1/provinces/{id} – retrieve a single province
        group.MapGet("/{provinceId:guid}", GetProvinceByIdAsync)
            .WithName("GetProvinceById")
            .Produces<ProvinceResponse>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);

        // POST /api/v1/provinces – create a new province (supports Idempotency-Key)
        group.MapPost("/", CreateProvinceAsync)
            .WithName("CreateProvince")
            .Produces<ProvinceResponse>(StatusCodes.Status201Created)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest)
            .Produces<ErrorEnvelope>(StatusCodes.Status409Conflict);

        // PUT /api/v1/provinces/{id} – full update
        group.MapPut("/{provinceId:guid}", UpdateProvinceAsync)
            .WithName("UpdateProvince")
            .Produces<ProvinceResponse>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);

        // DELETE /api/v1/provinces/{id} – delete a province
        group.MapDelete("/{provinceId:guid}", DeleteProvinceAsync)
            .WithName("DeleteProvince")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Handler for GET /provinces – returns a paginated, filterable, sortable list.
    /// </summary>
    private static async Task<IResult> GetProvincesAsync(
        [AsParameters] PaginationParams query, // Binds page, pageSize, search, sortBy, sortOrder from query string
        AppDbContext db,                      // Injected EF Core context
        HttpContext http,                     // Needed to build absolute HATEOAS links
        LinkGenerator linker)                 // ASP.NET Core helper to generate URLs
    {
        // Clamp page size to a reasonable range (prevents abuse)
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        // Start with the full Provinces set
        IQueryable<Province> source = db.Provinces.AsNoTracking();

        // Apply free-text search filter if provided (Filtering)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(p => p.ProvinceName.ToLower().Contains(term));
        }

        // Apply sorting (Sorting)
        source = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
        {
            ("name", "desc") => source.OrderByDescending(p => p.ProvinceName),
            ("name", _) => source.OrderBy(p => p.ProvinceName),
            ("createdon", "desc") => source.OrderByDescending(p => p.CreatedOn),
            ("createdon", _) => source.OrderBy(p => p.CreatedOn),
            _ => source.OrderBy(p => p.ProvinceName) // default sort
        };

        // Get total count before paging (for metadata)
        var total = await source.CountAsync();

        // Apply skip/take for the requested page (Pagination)
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProvinceResponse
            {
                ProvinceId = p.ProvinceId,
                ProvinceName = p.ProvinceName,
                CreatedBy = p.CreatedBy,
                CreatedOn = p.CreatedOn
            })
            .ToListAsync();

        // Build HATEOAS links for each item and for the collection
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        foreach (var item in items)
        {
            item.Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/provinces/{item.ProvinceId}", Method = "GET" },
                new HateoasLink { Rel = "update", Href = $"{baseUrl}/api/v1/provinces/{item.ProvinceId}", Method = "PUT" },
                new HateoasLink { Rel = "delete", Href = $"{baseUrl}/api/v1/provinces/{item.ProvinceId}", Method = "DELETE" }
            ];
        }

        // Construct the paginated response envelope
        var response = new PaginatedResponse<ProvinceResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        // Collection-level HATEOAS links (self, next, prev)
        response.Links.Add(new HateoasLink
        {
            Rel = "self",
            Href = $"{baseUrl}/api/v1/provinces?page={page}&pageSize={pageSize}",
            Method = "GET"
        });
        if (page * pageSize < total)
        {
            response.Links.Add(new HateoasLink
            {
                Rel = "next",
                Href = $"{baseUrl}/api/v1/provinces?page={page + 1}&pageSize={pageSize}",
                Method = "GET"
            });
        }
        if (page > 1)
        {
            response.Links.Add(new HateoasLink
            {
                Rel = "prev",
                Href = $"{baseUrl}/api/v1/provinces?page={page - 1}&pageSize={pageSize}",
                Method = "GET"
            });
        }

        // Return 200 OK with the paginated payload
        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for GET /provinces/{id}
    /// </summary>
    private static async Task<IResult> GetProvinceByIdAsync(
        Guid provinceId,
        AppDbContext db,
        HttpContext http)
    {
        // Look up the province; 404 if missing (Meaningful Status Codes)
        var province = await db.Provinces.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProvinceId == provinceId);

        if (province is null)
        {
            // Return a standard error envelope with 404
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Province with id '{provinceId}' was not found."
            });
        }

        // Map to response DTO and attach HATEOAS links
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new ProvinceResponse
        {
            ProvinceId = province.ProvinceId,
            ProvinceName = province.ProvinceName,
            CreatedBy = province.CreatedBy,
            CreatedOn = province.CreatedOn,
            Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/provinces/{province.ProvinceId}", Method = "GET" },
                new HateoasLink { Rel = "update", Href = $"{baseUrl}/api/v1/provinces/{province.ProvinceId}", Method = "PUT" },
                new HateoasLink { Rel = "delete", Href = $"{baseUrl}/api/v1/provinces/{province.ProvinceId}", Method = "DELETE" },
                new HateoasLink { Rel = "cities", Href = $"{baseUrl}/api/v1/cities?provinceId={province.ProvinceId}", Method = "GET" }
            ]
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for POST /provinces – creates a new province.
    /// </summary>
    private static async Task<IResult> CreateProvinceAsync(
        CreateProvinceRequest request,
        AppDbContext db,
        HttpContext http)
    {
        // Basic validation – return 400 with error envelope if name is missing
        if (string.IsNullOrWhiteSpace(request.ProvinceName))
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "ProvinceName is required.",
                Errors = new Dictionary<string, string[]>
                {
                    ["ProvinceName"] = ["The ProvinceName field is required."]
                }
            });
        }

        // Check for duplicate name (optional business rule) – 409 Conflict
        var exists = await db.Provinces.AnyAsync(p => p.ProvinceName == request.ProvinceName);
        if (exists)
        {
            return Results.Conflict(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = $"A province named '{request.ProvinceName}' already exists."
            });
        }

        // Create the entity
        var province = new Province
        {
            ProvinceId = Guid.NewGuid(),
            ProvinceName = request.ProvinceName.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "system" : request.CreatedBy!,
            CreatedOn = DateTime.UtcNow
        };

        // Persist
        db.Provinces.Add(province);
        await db.SaveChangesAsync();

        // Build response with location header and HATEOAS
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new ProvinceResponse
        {
            ProvinceId = province.ProvinceId,
            ProvinceName = province.ProvinceName,
            CreatedBy = province.CreatedBy,
            CreatedOn = province.CreatedOn,
            Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/provinces/{province.ProvinceId}", Method = "GET" }
            ]
        };

        // 201 Created with Location header pointing to the new resource
        return Results.Created($"/api/v1/provinces/{province.ProvinceId}", response);
    }

    /// <summary>
    /// Handler for PUT /provinces/{id}
    /// </summary>
    private static async Task<IResult> UpdateProvinceAsync(
        Guid provinceId,
        UpdateProvinceRequest request,
        AppDbContext db,
        HttpContext http)
    {
        var province = await db.Provinces.FindAsync(provinceId);
        if (province is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Province with id '{provinceId}' was not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.ProvinceName))
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "ProvinceName is required."
            });
        }

        // Apply the update
        province.ProvinceName = request.ProvinceName.Trim();
        await db.SaveChangesAsync();

        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new ProvinceResponse
        {
            ProvinceId = province.ProvinceId,
            ProvinceName = province.ProvinceName,
            CreatedBy = province.CreatedBy,
            CreatedOn = province.CreatedOn,
            Links =
            [
                new HateoasLink { Rel = "self", Href = $"{baseUrl}/api/v1/provinces/{province.ProvinceId}", Method = "GET" }
            ]
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for DELETE /provinces/{id}
    /// </summary>
    private static async Task<IResult> DeleteProvinceAsync(
        Guid provinceId,
        AppDbContext db,
        HttpContext http)
    {
        var province = await db.Provinces.FindAsync(provinceId);
        if (province is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Province with id '{provinceId}' was not found."
            });
        }

        // Remove and save
        db.Provinces.Remove(province);
        await db.SaveChangesAsync();

        // 204 No Content indicates successful deletion with no body
        return Results.NoContent();
    }
}
