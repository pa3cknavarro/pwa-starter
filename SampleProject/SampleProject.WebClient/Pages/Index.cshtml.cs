using SampleProject.Client.Models;
using SampleProject.Client.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SampleProject.WebClient.Pages;

public class IndexModel : PageModel
{
    private readonly SampleProjectClient _api;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(SampleProjectClient api, ILogger<IndexModel> logger)
    {
        _api = api;
        _logger = logger;
    }

    public string? ApiStatus { get; private set; }
    public int? ProvinceCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            var result = await _api.GetProvincesAsync(new ListQuery { Page = 1, PageSize = 1 });
            if (result.IsSuccess)
            {
                ApiStatus = "Connected";
                ProvinceCount = result.Data?.TotalCount;
            }
            else
            {
                ApiStatus = "Error";
                ErrorMessage = $"{result.StatusCode}: {result.Error?.Title} – {result.Error?.Detail}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reach SampleProject");
            ApiStatus = "Unreachable";
            ErrorMessage = ex.Message + " — Is SampleProject running? Check SampleProject:BaseUrl in appsettings.";
        }
    }
}
