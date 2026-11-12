namespace SampleProject.Client.Models;

public class BarangayResponse
{
    public Guid ProvinceId { get; set; }
    public Guid CityId { get; set; }
    public Guid BarangayId { get; set; }
    public string BarangayName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public List<HateoasLink> Links { get; set; } = new();
}

public class CreateBarangayRequest
{
    public Guid ProvinceId { get; set; }
    public Guid CityId { get; set; }
    public string BarangayName { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
}

public class UpdateBarangayRequest
{
    public string BarangayName { get; set; } = string.Empty;
}
