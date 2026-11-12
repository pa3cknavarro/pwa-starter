using SampleProject.Client.Models;
using SampleProject.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SampleProject.WebClient.Pages;

public class ProvincesModel : PageModel
{
    private readonly SampleProjectClient _api;

    public ProvincesModel(SampleProjectClient api) => _api = api;

    public PaginatedResponse<ProvinceResponse>? Result { get; private set; }
    public string? Error { get; private set; }
    public string? Message { get; private set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty]
    public string NewProvinceName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var res = await _api.GetProvincesAsync(new ListQuery
        {
            Page = PageNumber,
            PageSize = 10,
            Search = Search,
            SortBy = "name"
        });

        if (res.IsSuccess)
            Result = res.Data;
        else
            Error = FormatError(res);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProvinceName))
        {
            Error = "Province name is required.";
            await OnGetAsync();
            return Page();
        }

        var res = await _api.CreateProvinceAsync(new CreateProvinceRequest
        {
            ProvinceName = NewProvinceName.Trim(),
            CreatedBy = "web-client"
        }, Guid.NewGuid().ToString());

        if (res.IsSuccess)
            Message = $"Created province “{res.Data!.ProvinceName}”.";
        else
            Error = FormatError(res);

        NewProvinceName = string.Empty;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var res = await _api.DeleteProvinceAsync(id);
        if (res.IsSuccess)
            Message = "Province deleted.";
        else
            Error = FormatError(res);

        await OnGetAsync();
        return Page();
    }

    private static string FormatError<T>(ApiResult<T> r) =>
        $"{r.StatusCode}: {r.Error?.Title} – {r.Error?.Detail}";
}
