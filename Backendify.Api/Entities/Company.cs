namespace Backendify.Api.Entities
{
  /// <summary>
  /// Represents an internal company record.
  /// </summary>
  /// <param name="Id">The company identifier</param>
  /// <param name="CountryCode">The two digit country code.</param>
  /// <param name="CompanyName">The company name.</param>
  /// <param name="TaxId">The tax identifier.</param>
  /// <param name="Opened">The date the company started.</param>
  /// <param name="Closed">The date the company ended.</param>
  /// <param name="IsNullPlaceholder">Indicates whether this represents a 404 not-found reponse.</param>
  public record Company(string Id, string CountryCode, string CompanyName, string? TaxId, DateTime? Opened, DateTime? Closed, bool IsNullPlaceholder = false)
  {
  }
}
