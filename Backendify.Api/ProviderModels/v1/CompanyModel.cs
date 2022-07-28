using System.Text.Json.Serialization;

namespace Backendify.Api.ProviderModels.v1
{
  /// <summary>
  /// Represents an API version 1 company model.
  /// </summary>
  /// <param name="CompanyName">The company name.</param>
  /// <param name="CreatedOn">The date the company started.</param>
  /// <param name="ClosedOn">The date the company ended.</param>
  public record CompanyModel(
    [property: JsonPropertyName("cn")] string CompanyName,
    [property: JsonPropertyName("created_on")] string CreatedOn,
    [property: JsonPropertyName("closed_on")] string? ClosedOn)
  {
  }
}