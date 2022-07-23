using System.Text.Json.Serialization;
using System.Xml;

namespace Backendify.Api.Models.v2
{
  public record CompanyResultModel(
    [property: JsonPropertyName("company_name")] string CompanyName,
    [property: JsonPropertyName("tin")] string TaxId,
    [property: JsonIgnore] DateTime? Closed)
  {

    [property: JsonPropertyName("dissolved_on")]
    public string? ClosedOn => this.Closed.HasValue ? XmlConvert.ToString(this.Closed.Value, XmlDateTimeSerializationMode.Utc) : null;
  }
}
