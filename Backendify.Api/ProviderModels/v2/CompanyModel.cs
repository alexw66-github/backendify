using System.Text.Json.Serialization;

namespace Backendify.Api.ProviderModels.v2
{
  /// <summary>
  /// Represents an API version 2 company model.
  /// </summary>
  /// <param name="CompanyName">The company name.</param>
  /// <param name="TaxId">The tax identifier.</param>
  /// <param name="DissolvedOn">The date the company ended.</param>
  public record CompanyModel(
    [property: JsonPropertyName("company_name")] string CompanyName,
    [property: JsonPropertyName("tin")] string TaxId,
    [property: JsonPropertyName("dissolved_on")] string? DissolvedOn)
  {
  }
}