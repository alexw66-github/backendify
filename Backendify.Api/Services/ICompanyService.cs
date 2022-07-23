namespace Backendify.Api.Services
{
  public interface ICompanyService
  {
    Task<IResult> GetCompany(string id, string countryCode);
  }
}