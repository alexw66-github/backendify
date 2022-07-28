using Backendify.Api.Entities;

namespace Backendify.Api.Repositories
{
  /// <summary>
  /// Represents a database of companies.
  /// </summary>
  public interface ICompanyRepository
  {
    /// <summary>
    /// Tries to get the company with the specified identifiers.
    /// </summary>
    /// <param name="id">The company identifier.</param>
    /// <param name="countryCode">The country code.</param>
    /// <returns>Returns the cached company, or <c>null</c>.</returns>
    Company? TryGetCompany(string id, string countryCode);

    /// <summary>
    /// Tries to save the specified company.
    /// </summary>
    /// <param name="company">The company to save.</param>
    void TrySaveCompany(Company company);
  }
}