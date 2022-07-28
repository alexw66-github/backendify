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
          logger.LogDebug("A cache entry does not exist for specified company [{Id},{CountryCode}]", id, countryCode);

          match =
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

          try
          {
            if (await cache.Companies.FindAsync(id, countryCode) is not null)
            {
              logger.LogWarning("A matching company [{Id},{CountryCode}] has already been added or modified",match.Id, match.CountryCode);
            }
            else
            {
              await cache.Companies.AddAsync(match);
              await cache.SaveChangesAsync();
            }
          }
          catch (ArgumentException ex)
          {
            logger.LogWarning(ex, "A matching company [{Id},{CountryCode}] has already been added or modified: {Error}", match.Id, match.CountryCode, ex.Message);
          }
        }

        if (match.IsNullPlaceholder)
        {
          logger.LogInformation("Returning not found for company [{Id},{CountryCode}] after {Elapsed}", match.Id, match.CountryCode, timer.Elapsed);
          return Results.NotFound();
        }
        else
        {
          logger.LogInformation("Returning company \"{CompanyName}\" [{Id},{CountryCode}] after {Elapsed}", match.CompanyName, match.Id, match.CountryCode, timer.Elapsed);
          var result = new CompanyModel(match.Id, match.CompanyName, match.Closed);
          return Results.Ok(result);
        }
      }
    }
  }
}
