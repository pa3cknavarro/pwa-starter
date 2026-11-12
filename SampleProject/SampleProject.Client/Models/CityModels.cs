namespace SampleProject.Client.Models;

public class CityResponse
{
    public Guid ProvinceId { get; set; }
    public Guid CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public List<HateoasLink> Links { get; set; } = new();
}

public class CreateCityRequest
{
    public Guid ProvinceId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string? CreatedBy { get; set; }
}

public class UpdateCityRequest
{
    public string CityName { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
}
