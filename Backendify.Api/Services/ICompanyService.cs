namespace Backendify.Api.Services
{
  /// <summary>
  /// Represents a service for looking up companies.
  /// </summary>
  public interface ICompanyService
  {
    /// <summary>
    /// Gets the company with the specified identifier and country code.
    /// </summary>
    /// <param name="id">The company identifier.</param>
    /// <param name="countryCode">The country code.</param>
    /// <returns>Returns a HTTP result.</returns>
    Task<IResult> GetCompany(string id, string countryCode);
  }
}