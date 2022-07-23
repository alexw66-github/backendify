namespace Backendify.Api.Services.v1
{
  public interface ICompanyService
  {
    Task<IResult> GetCompanyByCountry(string id, string countryCode);
    Task<IResult> GetCompanyById(string id);
  }
}