using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services.External;
using Microsoft.EntityFrameworkCore;

namespace Backendify.Api.Services
{
  public class CompanyService : ICompanyService
  {
    private readonly CompanyRepository cache;
    private readonly IRemoteCompanyService remoteLookup;

    public CompanyService(CompanyRepository cache, IRemoteCompanyService remoteLookup)
    {
      this.cache = cache;
      this.remoteLookup = remoteLookup;
    }

    public async Task<IResult> GetCompany(string id, string countryCode)
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
        match = await remoteLookup.GetCompany(id, countryCode);

        if (match is null)
        {
          return Results.NotFound();
        }

        await cache.Companies.AddAsync(match);
      }

      var result = new CompanyModel(match.Id, match.CompanyName, match.Closed);
      return Results.Ok(result);
    }
  }
}
