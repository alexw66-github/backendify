namespace Backendify.Api.Entities
{
  public record Company(string Id, string CountryCode, string CompanyName, string TaxId, DateTime? Opened, DateTime? Closed)
  {
  }
}
