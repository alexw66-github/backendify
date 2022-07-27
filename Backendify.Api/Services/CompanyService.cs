using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services.External;
using System.Data;
using System.Diagnostics;

namespace Backendify.Api.Services
{
  public class CompanyService : ICompanyService
  {
    private readonly ICompanyRepository cache;
    private readonly IRemoteCompanyService remoteLookup;
    private readonly ILogger<CompanyService> logger;

    public CompanyService(ICompanyRepository cache, IRemoteCompanyService remoteLookup, ILogger<CompanyService> logger)
    {
      this.cache = cache;
      this.remoteLookup = remoteLookup;
      this.logger = logger;
    }

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
        var match = await cache.Companies.FindAsync(id, countryCode);

        if (match is null)
        {
          logger.LogInformation("A cache entry does not exist for specified company [{Id},{CountryCode}]", id, countryCode);

          match =
            await remoteLookup.GetCompany(id, countryCode) ??
            await cache.Companies.FindAsync(id, countryCode);

          if (match is null)
          {
            logger.LogError("Unable to locate the specified company [{Id},{CountryCode}] from downstream services after {Elapsed}", id, countryCode, timer.Elapsed);
            return Results.NotFound();
          }

          logger.LogInformation("Caching returned company \"{CompanyName}\" [{Id},{CountryCode}]", match.CompanyName, match.Id, match.CountryCode);
          logger.LogTrace("{@Company}", match);

          try
          {
            if (cache.Companies.Any(x => x.Id == match.Id && x.CountryCode == match.CountryCode))
            {
              logger.LogWarning("A matching company has already been added or modified");
            }
            else
            {
              await cache.Companies.AddAsync(match);
              await cache.SaveChangesAsync();
            }
          }
          catch (DBConcurrencyException ex)
          {
            logger.LogWarning(ex, "A matching company has already been added or modified: {Error}", ex.Message);
          }
        }
        else
        {
          logger.LogInformation("Existing cache entry found for the specified company \"{CompanyName}\" [{Id},{CountryCode}]", match.CompanyName, match.Id, match.CountryCode);
        }

        logger.LogInformation("Returning company \"{CompanyName}\" [{Id},{CountryCode}] after {Elapsed}", match.CompanyName, match.Id, match.CountryCode, timer.Elapsed);
        var result = new CompanyModel(match.Id, match.CompanyName, match.Closed);
        return Results.Ok(result);
      }
    }
  }
}
