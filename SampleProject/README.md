# Ecommerce Solution

Solution that demonstrates a **Vertical Slice API** (`SampleProject`) and **client applications** that consume it.

```
Ecommerce.sln
├── SampleProject              # Carter + vertical slices (Province, City, Barangay)
├── SampleProject.Client       # Typed HttpClient library (models + SampleProjectClient)
├── Ecommerce.ConsoleClient    # Console demo that calls the API
└── Ecommerce.WebClient        # Razor Pages UI that calls the API
```

## Projects

### SampleProject
Production-style API with:
Versioning, Pagination, Rate Limiting, Idempotency Keys, HATEOAS,
Meaningful Status Codes, Filtering + Sorting, Auth in Headers (`X-Api-Key`),
Error Envelopes, Vertical Slice + Carter.

```bash
cd SampleProject
dotnet run
# Note the HTTPS port (e.g. https://localhost:7xxx)
# Sample UI inside API: /samples/
```

### SampleProject.Client
Reusable class library:
- Response/request models matching the API
- `SampleProjectClient` typed service
- `AddSampleProjectClient(...)` DI extension
- Handles `X-Api-Key`, `X-Idempotency-Key`, camelCase JSON, `ErrorEnvelope`

### Ecommerce.ConsoleClient
Command-line sample:
1. Lists provinces
2. Creates a province (with idempotency key)
3. Creates a city and barangay under it

Update `ApiBaseUrl` in `Program.cs` to the API port, then:

```bash
cd Ecommerce.ConsoleClient
dotnet run
```

### Ecommerce.WebClient
Razor Pages app:
- Home – connection status to the API
- Provinces – list / create / delete
- Cities – list with province filter
- Barangays – list with province/city filters

Set `SampleProject:BaseUrl` in `appsettings.Development.json`, then:

```bash
cd Ecommerce.WebClient
dotnet run
```

## Typical workflow

1. Start **SampleProject** (`dotnet run`) and copy the HTTPS URL.
2. Put that URL into:
   - `Ecommerce.ConsoleClient/Program.cs` → `ApiBaseUrl`
   - `Ecommerce.WebClient/appsettings.Development.json` → `SampleProject:BaseUrl`
3. Run the console client and/or the web client.

API key used everywhere: `demo-api-key-12345`
