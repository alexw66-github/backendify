namespace Backendify.Api.Data
{
  public class Company
  {
    public string Id { get; set; }
    public string CountryCode { get; set; }
    public string CompanyName { get; set; }
    public string TaxId { get; set; }
    public DateTime Opened { get; set; }
    public DateTime? Closed { get; set; }
  }
}
