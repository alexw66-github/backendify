using Backendify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backendify.Api.Repositories
{
  public class CompanyRepository : DbContext, ICompanyRepository
  {
    public CompanyRepository(DbContextOptions<CompanyRepository> options)
        : base(options)
    { }

    public DbSet<Company> Companies { get; set; }
  }
}
