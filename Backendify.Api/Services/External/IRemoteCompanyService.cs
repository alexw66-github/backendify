using Backendify.Api.Entities;

namespace Backendify.Api.Services.External
{
  /// <summary>
  /// Represents a remote company lookup service.
  /// </summary>
  public interface IRemoteCompanyService
  {
    /// <summary>
    /// Gets the company with the specified identifier and country code.
    /// </summary>
    /// <param name="id">The company identifier.</param>
    /// <param name="countryCode">The country code.</param>
    /// <returns>Returns the matching company, or <c>null</c>.</returns>
    Task<Company?> GetCompany(string id, string countryCode);
  }
}