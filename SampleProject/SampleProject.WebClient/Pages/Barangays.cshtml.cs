using SampleProject.Client.Models;
using SampleProject.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleProject.WebClient.Pages;

public class BarangaysModel : PageModel
{
    private readonly SampleProjectClient _api;

    public BarangaysModel(SampleProjectClient api) => _api = api;

    public PaginatedResponse<BarangayResponse>? Result { get; private set; }
    public List<SelectListItem> ProvinceOptions { get; private set; } = new();
    public List<SelectListItem> CityOptions { get; private set; } = new();
    public string? Error { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ProvinceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CityId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var provinces = await _api.GetProvincesAsync(new ListQuery { PageSize = 100 });
        if (provinces.IsSuccess)
        {
            ProvinceOptions = provinces.Data!.Items
                .Select(p => new SelectListItem(p.ProvinceName, p.ProvinceId.ToString(), ProvinceId == p.ProvinceId))
                .ToList();
            ProvinceOptions.Insert(0, new SelectListItem("— All provinces —", ""));
        }

        var cities = await _api.GetCitiesAsync(new ListQuery { PageSize = 100 }, ProvinceId);
        if (cities.IsSuccess)
        {
            CityOptions = cities.Data!.Items
                .Select(c => new SelectListItem(c.CityName, c.CityId.ToString(), CityId == c.CityId))
                .ToList();
            CityOptions.Insert(0, new SelectListItem("— All cities —", ""));
        }

        var res = await _api.GetBarangaysAsync(
            new ListQuery { Page = PageNumber, PageSize = 10, SortBy = "name" },
            ProvinceId, CityId);

        if (res.IsSuccess)
            Result = res.Data;
        else
            Error = $"{res.StatusCode}: {res.Error?.Detail}";
    }
}
