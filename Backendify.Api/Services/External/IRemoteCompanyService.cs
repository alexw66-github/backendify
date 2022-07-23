using Backendify.Api.Entities;

namespace Backendify.Api.Services.External
{
  public interface IRemoteCompanyService
  {
    Task<Company> GetCompany(string id, string countryCode);
  }
}