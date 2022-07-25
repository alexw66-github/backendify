using Backendify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backendify.Api.Repositories
{
  public interface ICompanyRepository
  {
    DbSet<Company> Companies { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
  }
}