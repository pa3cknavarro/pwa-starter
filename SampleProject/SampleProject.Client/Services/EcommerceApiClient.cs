using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SampleProject.Client.Models;

namespace SampleProject.Client.Services;

/// <summary>
/// Typed HTTP client that talks to the SampleProject vertical-slice endpoints.
/// Handles API key, optional idempotency keys, JSON camelCase, and error envelopes.
/// </summary>
public class SampleProjectClient
{
    private readonly HttpClient _http;
    private readonly SampleProjectOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SampleProjectClient(HttpClient http, SampleProjectOptions options)
    {
        _http = http;
        _options = options;

        // Ensure BaseAddress and default headers are set once
        if (_http.BaseAddress is null && !string.IsNullOrWhiteSpace(_options.BaseUrl))
            _http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");

        if (!_http.DefaultRequestHeaders.Contains("X-Api-Key"))
            _http.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
    }

    // -------------------------------------------------------------------------
    // Provinces
    // -------------------------------------------------------------------------

    public Task<ApiResult<PaginatedResponse<ProvinceResponse>>> GetProvincesAsync(
        ListQuery? query = null, CancellationToken ct = default)
    {
        var path = BuildListPath("provinces", query);
        return GetAsync<PaginatedResponse<ProvinceResponse>>(path, ct);
    }

    public Task<ApiResult<ProvinceResponse>> GetProvinceAsync(Guid provinceId, CancellationToken ct = default)
        => GetAsync<ProvinceResponse>($"api/{_options.ApiVersion}/provinces/{provinceId}", ct);

    public Task<ApiResult<ProvinceResponse>> CreateProvinceAsync(
        CreateProvinceRequest request, string? idempotencyKey = null, CancellationToken ct = default)
        => PostAsync<ProvinceResponse>($"api/{_options.ApiVersion}/provinces", request, idempotencyKey, ct);

    public Task<ApiResult<ProvinceResponse>> UpdateProvinceAsync(
        Guid provinceId, UpdateProvinceRequest request, string? idempotencyKey = null, CancellationToken ct = default)
        => PutAsync<ProvinceResponse>($"api/{_options.ApiVersion}/provinces/{provinceId}", request, idempotencyKey, ct);

    public Task<ApiResult<object?>> DeleteProvinceAsync(
        Guid provinceId, string? idempotencyKey = null, CancellationToken ct = default)
        => DeleteAsync($"api/{_options.ApiVersion}/provinces/{provinceId}", idempotencyKey, ct);

    // -------------------------------------------------------------------------
    // Cities
    // -------------------------------------------------------------------------

    public Task<ApiResult<PaginatedResponse<CityResponse>>> GetCitiesAsync(
        ListQuery? query = null, Guid? provinceId = null, CancellationToken ct = default)
    {
        var extra = provinceId is null
            ? Array.Empty<(string, string)>()
            : new[] { ("provinceId", provinceId.Value.ToString()) };
        var path = BuildListPath("cities", query, extra);
        return GetAsync<PaginatedResponse<CityResponse>>(path, ct);
    }

    public Task<ApiResult<CityResponse>> GetCityAsync(Guid provinceId, Guid cityId, CancellationToken ct = default)
        => GetAsync<CityResponse>($"api/{_options.ApiVersion}/cities/{provinceId}/{cityId}", ct);

    public Task<ApiResult<CityResponse>> CreateCityAsync(
        CreateCityRequest request, string? idempotencyKey = null, CancellationToken ct = default)
        => PostAsync<CityResponse>($"api/{_options.ApiVersion}/cities", request, idempotencyKey, ct);

    public Task<ApiResult<CityResponse>> UpdateCityAsync(
        Guid provinceId, Guid cityId, UpdateCityRequest request, string? idempotencyKey = null, CancellationToken ct = default)
        => PutAsync<CityResponse>($"api/{_options.ApiVersion}/cities/{provinceId}/{cityId}", request, idempotencyKey, ct);

    public Task<ApiResult<object?>> DeleteCityAsync(
        Guid provinceId, Guid cityId, string? idempotencyKey = null, CancellationToken ct = default)
        => DeleteAsync($"api/{_options.ApiVersion}/cities/{provinceId}/{cityId}", idempotencyKey, ct);

    // -------------------------------------------------------------------------
    // Barangays
    // -------------------------------------------------------------------------

    public Task<ApiResult<PaginatedResponse<BarangayResponse>>> GetBarangaysAsync(
        ListQuery? query = null, Guid? provinceId = null, Guid? cityId = null, CancellationToken ct = default)
    {
        var extra = new List<(string, string)>();
        if (provinceId.HasValue) extra.Add(("provinceId", provinceId.Value.ToString()));
        if (cityId.HasValue) extra.Add(("cityId", cityId.Value.ToString()));
        var path = BuildListPath("barangays", query, extra.ToArray());
        return GetAsync<PaginatedResponse<BarangayResponse>>(path, ct);
    }

    public Task<ApiResult<BarangayResponse>> GetBarangayAsync(
        Guid provinceId, Guid cityId, Guid barangayId, CancellationToken ct = default)
        => GetAsync<BarangayResponse>(
            $"api/{_options.ApiVersion}/barangays/{provinceId}/{cityId}/{barangayId}", ct);

    public Task<ApiResult<BarangayResponse>> CreateBarangayAsync(
        CreateBarangayRequest request, string? idempotencyKey = null, CancellationToken ct = default)
        => PostAsync<BarangayResponse>($"api/{_options.ApiVersion}/barangays", request, idempotencyKey, ct);

    public Task<ApiResult<BarangayResponse>> UpdateBarangayAsync(
        Guid provinceId, Guid cityId, Guid barangayId, UpdateBarangayRequest request,
        string? idempotencyKey = null, CancellationToken ct = default)
        => PutAsync<BarangayResponse>(
            $"api/{_options.ApiVersion}/barangays/{provinceId}/{cityId}/{barangayId}",
            request, idempotencyKey, ct);

    public Task<ApiResult<object?>> DeleteBarangayAsync(
        Guid provinceId, Guid cityId, Guid barangayId, string? idempotencyKey = null, CancellationToken ct = default)
        => DeleteAsync($"api/{_options.ApiVersion}/barangays/{provinceId}/{cityId}/{barangayId}", idempotencyKey, ct);

    // -------------------------------------------------------------------------
    // HTTP helpers
    // -------------------------------------------------------------------------

    private static string BuildListPath(string resource, ListQuery? query, params (string Key, string Value)[] extra)
    {
        var q = query ?? new ListQuery();
        var sb = new StringBuilder($"api/v1/{resource}?page={q.Page}&pageSize={q.PageSize}");
        if (!string.IsNullOrWhiteSpace(q.Search)) sb.Append("&search=").Append(Uri.EscapeDataString(q.Search));
        if (!string.IsNullOrWhiteSpace(q.SortBy)) sb.Append("&sortBy=").Append(Uri.EscapeDataString(q.SortBy));
        if (!string.IsNullOrWhiteSpace(q.SortOrder)) sb.Append("&sortOrder=").Append(Uri.EscapeDataString(q.SortOrder));
        foreach (var (key, value) in extra)
            sb.Append('&').Append(key).Append('=').Append(Uri.EscapeDataString(value));
        return sb.ToString();
    }

    private async Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken ct)
    {
        using var res = await _http.GetAsync(path, ct);
        return await ReadResultAsync<T>(res, ct);
    }

    private async Task<ApiResult<T>> PostAsync<T>(string path, object body, string? idempotencyKey, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            req.Headers.TryAddWithoutValidation("X-Idempotency-Key", idempotencyKey);

        using var res = await _http.SendAsync(req, ct);
        return await ReadResultAsync<T>(res, ct);
    }

    private async Task<ApiResult<T>> PutAsync<T>(string path, object body, string? idempotencyKey, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            req.Headers.TryAddWithoutValidation("X-Idempotency-Key", idempotencyKey);

        using var res = await _http.SendAsync(req, ct);
        return await ReadResultAsync<T>(res, ct);
    }

    private async Task<ApiResult<object?>> DeleteAsync(string path, string? idempotencyKey, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Delete, path);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            req.Headers.TryAddWithoutValidation("X-Idempotency-Key", idempotencyKey);

        using var res = await _http.SendAsync(req, ct);
        if (res.IsSuccessStatusCode)
            return ApiResult<object?>.Success((int)res.StatusCode, null);

        ErrorEnvelope? error = null;
        try
        {
            error = await res.Content.ReadFromJsonAsync<ErrorEnvelope>(JsonOptions, ct);
        }
        catch { /* ignore parse errors */ }

        return ApiResult<object?>.Failure((int)res.StatusCode, error);
    }

    private static async Task<ApiResult<T>> ReadResultAsync<T>(HttpResponseMessage res, CancellationToken ct)
    {
        if (res.IsSuccessStatusCode)
        {
            // 204 No Content
            if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                return ApiResult<T>.Success((int)res.StatusCode, default!);

            var data = await res.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
            return ApiResult<T>.Success((int)res.StatusCode, data!);
        }

        ErrorEnvelope? error = null;
        try
        {
            error = await res.Content.ReadFromJsonAsync<ErrorEnvelope>(JsonOptions, ct);
        }
        catch { /* ignore */ }

        return ApiResult<T>.Failure((int)res.StatusCode, error);
    }
}
