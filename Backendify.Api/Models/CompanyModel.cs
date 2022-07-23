using System.Text.Json.Serialization;

namespace Backendify.Api.Models
{
  public record CompanyModel(string Id, string Name, [property: JsonPropertyName("active_until")] DateTime? ActiveUntil)
  {
    [JsonPropertyName("active")]
    public bool IsActive => this.ActiveUntil is null || this.ActiveUntil >= DateTime.Now;
  }
}