using SampleProject.Client.Models;
using SampleProject.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleProject.WebClient.Pages;

public class CitiesModel : PageModel
{
    private readonly SampleProjectClient _api;

    public CitiesModel(SampleProjectClient api) => _api = api;

    public PaginatedResponse<CityResponse>? Result { get; private set; }
    public List<SelectListItem> ProvinceOptions { get; private set; } = new();
    public string? Error { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ProvinceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var provinces = await _api.GetProvincesAsync(new ListQuery { PageSize = 100 });
        if (provinces.IsSuccess)
        {
            ProvinceOptions = provinces.Data!.Items
                .Select(p => new SelectListItem(p.ProvinceName, p.ProvinceId.ToString(),
                    ProvinceId == p.ProvinceId))
                .ToList();
            ProvinceOptions.Insert(0, new SelectListItem("— All —", ""));
        }

        var res = await _api.GetCitiesAsync(new ListQuery { Page = PageNumber, PageSize = 10, SortBy = "name" }, ProvinceId);
        if (res.IsSuccess)
            Result = res.Data;
        else
            Error = $"{res.StatusCode}: {res.Error?.Detail}";
    }
}
