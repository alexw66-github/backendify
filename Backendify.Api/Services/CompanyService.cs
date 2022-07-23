using Backendify.Api.Data;
using v1Models = Backendify.Api.Models.v1;
using v1Services = Backendify.Api.Services.v1;
using v2Models = Backendify.Api.Models.v2;
using v2Services = Backendify.Api.Services.v2;

namespace Backendify.Api.Services
{
  public class CompanyService : v1Services.ICompanyService, v2Services.ICompanyService
  {
    private readonly ICompanyRepository repository;

    public CompanyService(ICompanyRepository repository)
    {
      this.repository = repository;
    }

    async Task<IResult> v1Services.ICompanyService.GetCompanyByCountry(string id, string countryCode)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return Results.BadRequest($"\"{nameof(id)}\" is required.");
      }

      if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length > 2)
      {
        return Results.BadRequest("\"country_iso\" must be two characters.");
      }

      var match = await repository.GetByCountry(id, countryCode);

      if (match is null)
      {
        return Results.NotFound();
      }

      var result = new v1Models.RegionalCompanyModel(match.Id, match.CompanyName, match.Closed);
      return Results.Ok(result);
    }

    async Task<IResult> v1Services.ICompanyService.GetCompanyById(string id)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return Results.BadRequest($"\"{nameof(id)}\" is required.");
      }

      var match = await repository.GetById(id);

      if (match is null)
      {
        return Results.NotFound();
      }

      var result = new v1Models.CompanyResultModel(match.CompanyName, match.Opened, match.Closed);
      return Results.Ok(result);
    }

    async Task<IResult> v2Services.ICompanyService.GetCompanyById(string id)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return Results.BadRequest($"\"{nameof(id)}\" is required.");
      }

      var match = await repository.GetById(id);

      if (match is null)
      {
        return Results.NotFound();
      }

      var result = new v2Models.CompanyResultModel(match.CompanyName, match.TaxId, match.Closed);
      return Results.Ok(result);
    }
  }
}
