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
        var company = await cache.Companies.FindAsync(id, countryCode);

        if (company is null)
        {
          logger.LogDebug("A cache entry does not exist for specified company [{Id},{CountryCode}]", id, countryCode);
          company = await GetCompanyFromRemoteService(id, countryCode);
          await this.CacheResult(id, countryCode, company);
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

    private async Task<Company> GetCompanyFromRemoteService(string id, string countryCode)
    {
      var match =
        await remoteLookup.GetCompany(id, countryCode) ??
        await cache.Companies.FindAsync(id, countryCode);

      if (match is null || match.IsNullPlaceholder)
      {
        logger.LogError("Unable to locate the specified company [{Id},{CountryCode}] from downstream services", id, countryCode);
        match = new Entities.Company(id, countryCode, string.Empty, null, null, null, IsNullPlaceholder: true);
      }
      else
      {
        logger.LogInformation("Caching returned company \"{CompanyName}\" [{Id},{CountryCode}]", match.CompanyName, match.Id, match.CountryCode);
        logger.LogTrace("{@Company}", match);
      }

      return match;
    }

    private async Task CacheResult(string id, string countryCode, Company? match)
    {
      if (match is null)
      {
        return;
      }

      try
      {
        if (await cache.Companies.FindAsync(id, countryCode) is not null)
        {
          logger.LogWarning("A matching company [{Id},{CountryCode}] has already been added or modified", match.Id, match.CountryCode);
        }
        else
        {
          cache.Companies.Add(match);
          await cache.SaveChangesAsync();
        }
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "A matching company [{Id},{CountryCode}] has already been added or modified: {Error}", match.Id, match.CountryCode, ex.Message);
      }
    }
  }
}
