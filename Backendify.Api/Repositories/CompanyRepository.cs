using Backendify.Api.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Backendify.Api.Repositories
{
  /// <summary>
  /// Database of companies.
  /// </summary>
  /// <seealso cref="DbContext" />
  /// <seealso cref="ICompanyRepository" />
  public class CompanyRepository : ICompanyRepository
  {
    private readonly IMemoryCache innerCache;
    private readonly ILogger<CompanyRepository> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyRepository" /> class.
    /// </summary>
    /// <param name="innerCache">The inner cache.</param>
    /// <param name="logger">The logger to use.</param>
    public CompanyRepository(IMemoryCache innerCache, ILogger<CompanyRepository> logger)
    {
      this.innerCache = innerCache;
      this.logger = logger;
    }

    public Company? TryGetCompany(string id, string countryCode)
    {
      if (this.innerCache.TryGetValue($"{id}:{countryCode}", out Company value))
      {
        this.logger.LogDebug("Found a cache entry for company [{Id},{CountryCode}]", id, countryCode);
        return value;
      }
      else
      {
        this.logger.LogDebug("A cache entry does not exist for specified company [{Id},{CountryCode}]", id, countryCode);
        return null;
      }
    }

    public void TrySaveCompany(Company company)
    {
      MemoryCacheEntryOptions options = new()
      {
        SlidingExpiration = TimeSpan.FromDays(1),
      };

      this.innerCache.Set($"{company.Id}:{company.CountryCode}", company, options);

      if (company.IsNullPlaceholder)
      {
        logger.LogDebug("Caching null company [{Id},{CountryCode}]", company.Id, company.CountryCode);
      }
      else
      {
        logger.LogDebug("Caching company \"{CompanyName}\" [{Id},{CountryCode}]", company.CompanyName, company.Id, company.CountryCode);
      }
    }
  }
}
