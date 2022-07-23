namespace Backendify.Api.Data
{
  public interface ICompanyRepository
  {
    public bool IsReady { get; }
    Task<Company> GetByCountry(string id, string countryCode);
    Task<Company> GetById(string id);
  }
}