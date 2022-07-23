using Backendify.Api.Data;
using Backendify.Api.Services.v1;

namespace Backendify.Api.Services
{
  public class StatusService : IStatusService
  {
    private readonly ICompanyRepository repository;

    public StatusService(ICompanyRepository repository)
    {
      this.repository = repository;
    }

    public IResult GetStatus()
    {
      return repository.IsReady ? Results.Ok() : Results.NoContent();
    }
  }
}