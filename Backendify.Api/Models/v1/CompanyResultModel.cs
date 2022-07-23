using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml;

namespace Backendify.Api.Models.v1
{
  public record CompanyResultModel(
    [property: JsonPropertyName("cn")] string CompanyName,
    [property: JsonIgnore] DateTime Opened,
    [property: JsonIgnore] DateTime? Closed)
  {
    [JsonPropertyName("created_on")]
    public string CreatedOn => XmlConvert.ToString(this.Opened, XmlDateTimeSerializationMode.Utc);

    [JsonPropertyName("closed_on")]
    public string? ClosedOn => this.Closed.HasValue ? XmlConvert.ToString(this.Closed.Value, XmlDateTimeSerializationMode.Utc) : null;
  }
}