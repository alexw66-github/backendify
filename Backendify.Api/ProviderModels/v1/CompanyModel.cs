using System.Text.Json.Serialization;

namespace Backendify.Api.ProviderModels.v1
{
  public record CompanyModel(
    [property: JsonPropertyName("cn")] string CompanyName,
    [property: JsonPropertyName("created_on")] string CreatedOn,
    [property: JsonPropertyName("closed_on")] string? ClosedOn)
  {
  }
}