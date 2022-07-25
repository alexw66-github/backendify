using System.Text.Json.Serialization;
using System.Xml;

namespace Backendify.Api.Models
{
  public record CompanyModel(string Id, string Name, [property: JsonIgnore] DateTime? Closed)
  {
    [JsonPropertyName("active")]
    public bool IsActive => this.Closed is null || this.Closed >= DateTime.UtcNow;

    [JsonPropertyName("active_until")]
    public string? ActiveUntil => this.Closed is not null ? XmlConvert.ToString(this.Closed.Value, XmlDateTimeSerializationMode.Utc) : null;
  }
}