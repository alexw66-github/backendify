﻿using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services.External;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        var match = await cache.Companies.SingleOrDefaultAsync(x => x.Id == id && x.CountryCode == countryCode);

        if (match is null)
        {
          logger.LogDebug("A cache entry does not exist for specified company");

          match = 
            await remoteLookup.GetCompany(id, countryCode) ?? 
            await cache.Companies.SingleOrDefaultAsync(x => x.Id == id && x.CountryCode == countryCode);

          if (match is null)
          {
            logger.LogError("Unable to locate the specified company from downstream services");
            return Results.NotFound();
          }

          logger.LogDebug("Caching discovered company");
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
          logger.LogDebug("Existing cache entry found for the specified company");
        }

        logger.LogInformation("Returning company {CompanyName} [{Id},{CountryCode}]", match.CompanyName, match.Id, match.CountryCode);
        var result = new CompanyModel(match.Id, match.CompanyName, match.Closed);
        return Results.Ok(result);
      }
    }
  }
}
