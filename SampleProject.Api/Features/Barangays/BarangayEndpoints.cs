// Namespace for the Barangays vertical slice
namespace Ecommerce.Api.Features.Barangays;

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
/// Carter module that registers all endpoints for the Barangays feature.
/// Demonstrates the same cross-cutting concerns as Province and City:
/// Versioning, Pagination, Filtering + Sorting, HATEOAS, Meaningful Status Codes,
/// Idempotency (via middleware), Auth in Headers, Error Envelopes.
/// </summary>
public class BarangayEndpoints : ICarterModule
{
    // Called by Carter at startup to map the routes for this module
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Versioned route group: /api/v{version}/barangays
        var group = app.NewVersionedApi()
            .MapGroup("/api/v{version:apiVersion}/barangays")
            .HasApiVersion(1.0)
            .WithTags("Barangays");

        // GET /api/v1/barangays – list with optional province/city filters, pagination, sorting
        group.MapGet("/", GetBarangaysAsync)
            .WithName("GetBarangays")
            .Produces<PaginatedResponse<BarangayResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest);

        // GET /api/v1/barangays/{provinceId}/{cityId}/{barangayId} – single barangay by composite key
        group.MapGet("/{provinceId:guid}/{cityId:guid}/{barangayId:guid}", GetBarangayByIdAsync)
            .WithName("GetBarangayById")
            .Produces<BarangayResponse>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);

        // POST /api/v1/barangays – create a new barangay (Idempotency-Key supported via middleware)
        group.MapPost("/", CreateBarangayAsync)
            .WithName("CreateBarangay")
            .Produces<BarangayResponse>(StatusCodes.Status201Created)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest)
            .Produces<ErrorEnvelope>(StatusCodes.Status409Conflict);

        // PUT /api/v1/barangays/{provinceId}/{cityId}/{barangayId} – full update
        group.MapPut("/{provinceId:guid}/{cityId:guid}/{barangayId:guid}", UpdateBarangayAsync)
            .WithName("UpdateBarangay")
            .Produces<BarangayResponse>(StatusCodes.Status200OK)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound)
            .Produces<ErrorEnvelope>(StatusCodes.Status400BadRequest);

        // DELETE /api/v1/barangays/{provinceId}/{cityId}/{barangayId}
        group.MapDelete("/{provinceId:guid}/{cityId:guid}/{barangayId:guid}", DeleteBarangayAsync)
            .WithName("DeleteBarangay")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorEnvelope>(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Handler for GET /barangays – returns a paginated, filterable, sortable list.
    /// Optional query parameters: provinceId, cityId.
    /// </summary>
    private static async Task<IResult> GetBarangaysAsync(
        [AsParameters] PaginationParams query, // page, pageSize, search, sortBy, sortOrder
        Guid? provinceId,                     // optional parent province filter
        Guid? cityId,                         // optional parent city filter
        AppDbContext db,
        HttpContext http)
    {
        // Clamp page size to a safe range
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(query.Page, 1);

        // Start with all barangays (no tracking for read performance)
        IQueryable<Barangay> source = db.Barangays.AsNoTracking();

        // Filter by parent province when supplied
        if (provinceId.HasValue)
            source = source.Where(b => b.ProvinceId == provinceId.Value);

        // Filter by parent city when supplied
        if (cityId.HasValue)
            source = source.Where(b => b.CityId == cityId.Value);

        // Free-text search on barangay name
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(b => b.BarangayName.ToLower().Contains(term));
        }

        // Apply sorting
        source = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
        {
            ("name", "desc") => source.OrderByDescending(b => b.BarangayName),
            ("name", _) => source.OrderBy(b => b.BarangayName),
            ("createdon", "desc") => source.OrderByDescending(b => b.CreatedOn),
            ("createdon", _) => source.OrderBy(b => b.CreatedOn),
            _ => source.OrderBy(b => b.BarangayName)
        };

        // Total count before paging (needed for pagination metadata)
        var total = await source.CountAsync();

        // Fetch the requested page and project to the response DTO
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BarangayResponse
            {
                ProvinceId = b.ProvinceId,
                CityId = b.CityId,
                BarangayId = b.BarangayId,
                BarangayName = b.BarangayName,
                CreatedBy = b.CreatedBy,
                CreatedOn = b.CreatedOn
            })
            .ToListAsync();

        // Build absolute base URL for HATEOAS links
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";

        // Attach per-item links
        foreach (var item in items)
        {
            item.Links =
            [
                new HateoasLink
                {
                    Rel = "self",
                    Href = $"{baseUrl}/api/v1/barangays/{item.ProvinceId}/{item.CityId}/{item.BarangayId}",
                    Method = "GET"
                },
                new HateoasLink
                {
                    Rel = "update",
                    Href = $"{baseUrl}/api/v1/barangays/{item.ProvinceId}/{item.CityId}/{item.BarangayId}",
                    Method = "PUT"
                },
                new HateoasLink
                {
                    Rel = "delete",
                    Href = $"{baseUrl}/api/v1/barangays/{item.ProvinceId}/{item.CityId}/{item.BarangayId}",
                    Method = "DELETE"
                },
                new HateoasLink
                {
                    Rel = "city",
                    Href = $"{baseUrl}/api/v1/cities/{item.ProvinceId}/{item.CityId}",
                    Method = "GET"
                },
                new HateoasLink
                {
                    Rel = "province",
                    Href = $"{baseUrl}/api/v1/provinces/{item.ProvinceId}",
                    Method = "GET"
                }
            ];
        }

        // Build the paginated response envelope
        var response = new PaginatedResponse<BarangayResponse>
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
            Href = $"{baseUrl}/api/v1/barangays?page={page}&pageSize={pageSize}",
            Method = "GET"
        });
        if (page * pageSize < total)
        {
            response.Links.Add(new HateoasLink
            {
                Rel = "next",
                Href = $"{baseUrl}/api/v1/barangays?page={page + 1}&pageSize={pageSize}",
                Method = "GET"
            });
        }
        if (page > 1)
        {
            response.Links.Add(new HateoasLink
            {
                Rel = "prev",
                Href = $"{baseUrl}/api/v1/barangays?page={page - 1}&pageSize={pageSize}",
                Method = "GET"
            });
        }

        // 200 OK with the paginated payload
        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for GET /barangays/{provinceId}/{cityId}/{barangayId}
    /// </summary>
    private static async Task<IResult> GetBarangayByIdAsync(
        Guid provinceId,
        Guid cityId,
        Guid barangayId,
        AppDbContext db,
        HttpContext http)
    {
        var barangay = await db.Barangays.AsNoTracking()
            .FirstOrDefaultAsync(b =>
                b.ProvinceId == provinceId &&
                b.CityId == cityId &&
                b.BarangayId == barangayId);

        // 404 with standard error envelope when not found
        if (barangay is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Barangay with ProvinceId '{provinceId}', CityId '{cityId}' and BarangayId '{barangayId}' was not found."
            });
        }

        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new BarangayResponse
        {
            ProvinceId = barangay.ProvinceId,
            CityId = barangay.CityId,
            BarangayId = barangay.BarangayId,
            BarangayName = barangay.BarangayName,
            CreatedBy = barangay.CreatedBy,
            CreatedOn = barangay.CreatedOn,
            Links =
            [
                new HateoasLink
                {
                    Rel = "self",
                    Href = $"{baseUrl}/api/v1/barangays/{barangay.ProvinceId}/{barangay.CityId}/{barangay.BarangayId}",
                    Method = "GET"
                },
                new HateoasLink
                {
                    Rel = "update",
                    Href = $"{baseUrl}/api/v1/barangays/{barangay.ProvinceId}/{barangay.CityId}/{barangay.BarangayId}",
                    Method = "PUT"
                },
                new HateoasLink
                {
                    Rel = "delete",
                    Href = $"{baseUrl}/api/v1/barangays/{barangay.ProvinceId}/{barangay.CityId}/{barangay.BarangayId}",
                    Method = "DELETE"
                },
                new HateoasLink
                {
                    Rel = "city",
                    Href = $"{baseUrl}/api/v1/cities/{barangay.ProvinceId}/{barangay.CityId}",
                    Method = "GET"
                },
                new HateoasLink
                {
                    Rel = "province",
                    Href = $"{baseUrl}/api/v1/provinces/{barangay.ProvinceId}",
                    Method = "GET"
                }
            ]
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for POST /barangays – creates a new barangay under an existing city.
    /// </summary>
    private static async Task<IResult> CreateBarangayAsync(
        CreateBarangayRequest request,
        AppDbContext db,
        HttpContext http)
    {
        // Validation – return 400 with field-level errors when name is missing
        if (string.IsNullOrWhiteSpace(request.BarangayName))
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "BarangayName is required.",
                Errors = new Dictionary<string, string[]>
                {
                    ["BarangayName"] = ["The BarangayName field is required."]
                }
            });
        }

        // Ensure the parent city exists (which also implies the province exists)
        var cityExists = await db.Cities.AnyAsync(c =>
            c.ProvinceId == request.ProvinceId &&
            c.CityId == request.CityId);
        if (!cityExists)
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = $"City with ProvinceId '{request.ProvinceId}' and CityId '{request.CityId}' does not exist."
            });
        }

        // Optional uniqueness check (same name under the same city)
        var duplicate = await db.Barangays.AnyAsync(b =>
            b.ProvinceId == request.ProvinceId &&
            b.CityId == request.CityId &&
            b.BarangayName == request.BarangayName.Trim());
        if (duplicate)
        {
            return Results.Conflict(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = $"A barangay named '{request.BarangayName}' already exists in this city."
            });
        }

        // Create the entity
        var barangay = new Barangay
        {
            ProvinceId = request.ProvinceId,
            CityId = request.CityId,
            BarangayId = Guid.NewGuid(),
            BarangayName = request.BarangayName.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "system" : request.CreatedBy!,
            CreatedOn = DateTime.UtcNow
        };

        db.Barangays.Add(barangay);
        await db.SaveChangesAsync();

        // Build response with HATEOAS and Location header
        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new BarangayResponse
        {
            ProvinceId = barangay.ProvinceId,
            CityId = barangay.CityId,
            BarangayId = barangay.BarangayId,
            BarangayName = barangay.BarangayName,
            CreatedBy = barangay.CreatedBy,
            CreatedOn = barangay.CreatedOn,
            Links =
            [
                new HateoasLink
                {
                    Rel = "self",
                    Href = $"{baseUrl}/api/v1/barangays/{barangay.ProvinceId}/{barangay.CityId}/{barangay.BarangayId}",
                    Method = "GET"
                }
            ]
        };

        // 201 Created
        return Results.Created(
            $"/api/v1/barangays/{barangay.ProvinceId}/{barangay.CityId}/{barangay.BarangayId}",
            response);
    }

    /// <summary>
    /// Handler for PUT /barangays/{provinceId}/{cityId}/{barangayId}
    /// </summary>
    private static async Task<IResult> UpdateBarangayAsync(
        Guid provinceId,
        Guid cityId,
        Guid barangayId,
        UpdateBarangayRequest request,
        AppDbContext db,
        HttpContext http)
    {
        var barangay = await db.Barangays.FirstOrDefaultAsync(b =>
            b.ProvinceId == provinceId &&
            b.CityId == cityId &&
            b.BarangayId == barangayId);

        if (barangay is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Barangay with ProvinceId '{provinceId}', CityId '{cityId}' and BarangayId '{barangayId}' was not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.BarangayName))
        {
            return Results.BadRequest(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "BarangayName is required."
            });
        }

        // Apply changes
        barangay.BarangayName = request.BarangayName.Trim();
        await db.SaveChangesAsync();

        var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";
        var response = new BarangayResponse
        {
            ProvinceId = barangay.ProvinceId,
            CityId = barangay.CityId,
            BarangayId = barangay.BarangayId,
            BarangayName = barangay.BarangayName,
            CreatedBy = barangay.CreatedBy,
            CreatedOn = barangay.CreatedOn,
            Links =
            [
                new HateoasLink
                {
                    Rel = "self",
                    Href = $"{baseUrl}/api/v1/barangays/{barangay.ProvinceId}/{barangay.CityId}/{barangay.BarangayId}",
                    Method = "GET"
                }
            ]
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Handler for DELETE /barangays/{provinceId}/{cityId}/{barangayId}
    /// </summary>
    private static async Task<IResult> DeleteBarangayAsync(
        Guid provinceId,
        Guid cityId,
        Guid barangayId,
        AppDbContext db,
        HttpContext http)
    {
        var barangay = await db.Barangays.FirstOrDefaultAsync(b =>
            b.ProvinceId == provinceId &&
            b.CityId == cityId &&
            b.BarangayId == barangayId);

        if (barangay is null)
        {
            return Results.NotFound(new ErrorEnvelope
            {
                TraceId = http.TraceIdentifier,
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Barangay with ProvinceId '{provinceId}', CityId '{cityId}' and BarangayId '{barangayId}' was not found."
            });
        }

        db.Barangays.Remove(barangay);
        await db.SaveChangesAsync();

        // 204 No Content – successful deletion with empty body
        return Results.NoContent();
    }
}
