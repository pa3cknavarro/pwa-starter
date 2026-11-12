namespace SampleProject.Client.Models;

public class ProvinceResponse
{
    public Guid ProvinceId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public List<HateoasLink> Links { get; set; } = new();
}

public class CreateProvinceRequest
{
    public string ProvinceName { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
}

public class UpdateProvinceRequest
{
    public string ProvinceName { get; set; } = string.Empty;
}
