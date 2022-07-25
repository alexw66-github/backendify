using System.Text.Json.Serialization;

namespace Backendify.Api.ProviderModels.v2
{
  public record CompanyModel(
    [property: JsonPropertyName("company_name")] string CompanyName,
    [property: JsonPropertyName("tin")] string TaxId,
    [property: JsonPropertyName("dissolved_on")] string? DissolvedOn)
  {
  }
}