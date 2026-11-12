# SampleProject – Vertical Slice + Carter Sample

A fully commented ASP.NET Core 8 Web API demonstrating **Vertical Slice Architecture** with **Carter**, plus the following production-oriented features:

| Feature                | Implementation                                                                 |
|------------------------|--------------------------------------------------------------------------------|
| **Versioning**         | Asp.Versioning – URL segment (`/api/v1/...`), header & query string            |
| **Pagination**         | `page` + `pageSize` query params + `PaginatedResponse<T>`                      |
| **Rate Limiting**      | Fixed-window limiter (100 req/min per IP) with 429 + ErrorEnvelope            |
| **Idempotency Keys**   | `X-Idempotency-Key` header – responses cached in-memory for 24 h               |
| **HATEOAS**            | `links` array on every resource & collection (`self`, `next`, `prev`, …)       |
| **Meaningful Statuses**| 200, 201, 204, 400, 401, 404, 409, 429, 500                                    |
| **Filtering + Sorting**| `search`, `sortBy`, `sortOrder` query parameters                               |
| **Consistent Naming**  | camelCase JSON via `JsonNamingPolicy.CamelCase`                                |
| **Auth in Headers**    | Required `X-Api-Key: demo-api-key-12345`                                       |
| **Error Envelopes**    | Uniform `ErrorEnvelope` shape for every error response                         |

## Project Structure (Vertical Slices)

```
SampleProject/
├── Common/                 # Shared DTOs, helpers, constants
├── Data/                   # AppDbContext
├── Features/
│   ├── Provinces/          # Full CRUD slice (endpoints + DTOs)
│   ├── Cities/             # List / Get / Create slice
│   └── Barangays/          # List / Get / Create slice
├── Middleware/             # ApiKey, Idempotency, ExceptionHandling
├── Models/                 # EF Core entities (Province, City, Barangay)
└── Program.cs              # Composition root – all cross-cutting concerns
```

Each feature folder is a **vertical slice**: it owns its routes, request/response shapes and handler logic. No traditional “Controllers / Services / Repositories” layering.

## Quick Start

```bash
cd SampleProject
dotnet run
```

Swagger UI: https://localhost:7xxx/swagger (or the port shown in the console)

### Required Header on every request

```
X-Api-Key: demo-api-key-12345
```

### Example – List Provinces (paginated + filtered + sorted)

```
GET /api/v1/provinces?page=1&pageSize=10&search=metro&sortBy=name&sortOrder=asc
X-Api-Key: demo-api-key-12345
```

### Example – Create Province (with Idempotency)

```
POST /api/v1/provinces
X-Api-Key: demo-api-key-12345
X-Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json

{
  "provinceName": "Davao del Sur",
  "createdBy": "admin"
}
```

Repeating the same request with the same `X-Idempotency-Key` returns the original 201 response instead of creating a duplicate.

## Seed Data

On first start the InMemory database is seeded with:

- Province “Metro Manila” + City “Makati” + Barangay “Poblacion”
- Province “Cebu”

## Notes

- InMemory EF Core is used for zero-setup demos. Swap to SQL Server / PostgreSQL by changing the connection string and provider package.
- The IdempotencyStore is process-local; replace with Redis (or a database table with TTL) for multi-instance deployments.
- Rate-limit partition key is currently the client IP; you can change it to the API key or a user id.
