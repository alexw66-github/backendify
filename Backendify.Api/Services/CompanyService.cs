using Backendify.Api.Entities;
using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services.External;
using System.Diagnostics;

namespace Backendify.Api.Services
{
  /// <summary>
  /// Represents a service for looking up companies.
  /// </summary>
  /// <seealso cref="ICompanyService" />
  public class CompanyService : ICompanyService
  {
    private readonly ICompanyRepository cache;
    private readonly IRemoteCompanyService remoteLookup;
    private readonly ILogger<CompanyService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyService"/> class.
    /// </summary>
    /// <param name="cache">The cache to use.</param>
    /// <param name="remoteLookup">The remote company lookup.</param>
    /// <param name="logger">The logger to use.</param>
    public CompanyService(ICompanyRepository cache, IRemoteCompanyService remoteLookup, ILogger<CompanyService> logger)
    {
      this.cache = cache;
      this.remoteLookup = remoteLookup;
      this.logger = logger;
    }

    /// <summary>
    /// Gets the company with the specified identifier and country code.
    /// </summary>
    /// <param name="id">The company identifier.</param>
    /// <param name="countryCode">The country code.</param>
    /// <returns>
    /// Returns a HTTP result.
    /// </returns>
    public async Task<IResult> GetCompany(string id, string countryCode)
    {
      using (logger.BeginScope("Id={id}, CountryCode={countryCode}", id, countryCode))
      {
        if (string.IsNullOrWhiteSpace(id))
        {
          return Results.BadRequest($"\"{nameof(id)}\" is required.");
        }

        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length > 2)
        {
          return Results.BadRequest("\"country_iso\" must be two characters.");
        }

        var timer = Stopwatch.StartNew();
        var company = this.cache.TryGetCompany(id, countryCode);

        if (company is null)
        {
          company = 
            await this.TryGetCompanyFromRemoteService(id, countryCode) ??
            this.cache.TryGetCompany(id, countryCode);

          this.CacheResult(company);
        }

        if (company is null || company.IsNullPlaceholder)
        {
          logger.LogInformation("Returning not-found (404) result for company [{Id},{CountryCode}] after {Elapsed}", company.Id, company.CountryCode, timer.Elapsed);
          return Results.NotFound();
        }
        else
        {
          logger.LogInformation("Returning found (200) result for company \"{CompanyName}\" [{Id},{CountryCode}] after {Elapsed}", company.CompanyName, company.Id, company.CountryCode, timer.Elapsed);
          var result = new CompanyModel(company.Id, company.CompanyName, company.Closed);
          return Results.Ok(result);
        }
      }
    }

    private async Task<Company> TryGetCompanyFromRemoteService(string id, string countryCode)
    {
      var company = await remoteLookup.GetCompany(id, countryCode);

      if (company is null || company.IsNullPlaceholder)
      {
        logger.LogWarning("Unable to locate the specified company [{Id},{CountryCode}] from downstream services", id, countryCode);
        company = new Entities.Company(id, countryCode, string.Empty, null, null, null, IsNullPlaceholder: true);
      }

      return company;
    }

    private void CacheResult(Company? company)
    {
      if (company is null)
      {
        return;
      }

      if (company.IsNullPlaceholder)
      {
        logger.LogInformation("Caching null company [{Id},{CountryCode}]", company.Id, company.CountryCode);
      }
      else
      {
        logger.LogInformation("Caching company \"{CompanyName}\" [{Id},{CountryCode}]", company.CompanyName, company.Id, company.CountryCode);
      }

      logger.LogTrace("{@Company}", company);

      this.cache.TrySaveCompany(company);
    }
  }
}
