using System.Text.Json.Serialization;
using System.Xml;

namespace Backendify.Api.Models
{
  /// <summary>
  /// Represents a publicly returned company record.
  /// </summary>
  /// <param name="Id">The company identifer.</param>
  /// <param name="Name">The company name.</param>
  /// <param name="Closed">The date the company ended.</param>
  public record struct CompanyModel(string Id, string Name, [property: JsonIgnore] DateTime? Closed)
  {
    [JsonPropertyName("active")]
    public bool IsActive => this.Closed is null || this.Closed >= DateTime.UtcNow;

    [JsonPropertyName("active_until")]
    public string? ActiveUntil => this.Closed is not null ? XmlConvert.ToString(this.Closed.Value, XmlDateTimeSerializationMode.Utc) : null;
  }
}