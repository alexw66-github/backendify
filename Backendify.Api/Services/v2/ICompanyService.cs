namespace Backendify.Api.Services.v2
{
  public interface ICompanyService
  {
    Task<IResult> GetCompanyById(string id);
  }
}