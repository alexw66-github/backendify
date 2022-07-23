namespace Backendify.Api.Data
{
  public class CompanyRepository : ICompanyRepository
  {
    private readonly IEnumerable<Company> values;

    public CompanyRepository(IEnumerable<Company> values)
    {
      this.values = values ?? Enumerable.Empty<Company>();
    }

    public bool IsReady => values is not null;

    public Task<Company?> GetByCountry(string id, string countryCode) => Task.FromResult(values.FirstOrDefault(x => x.Id == id && x.CountryCode == countryCode));

    public Task<Company?> GetById(string id) => Task.FromResult(values.FirstOrDefault(x => x.Id == id));
  }
}
