using Backendify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backendify.Api.Repositories
{
  public class CompanyRepository : DbContext, ICompanyRepository
  {
    public CompanyRepository(DbContextOptions<CompanyRepository> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Company>()
          .HasKey(new[] { nameof(Company.Id), nameof(Company.CountryCode) });

      modelBuilder.Entity<Company>().Property(x=>x.Id)
        .ValueGeneratedNever();
    }

    public DbSet<Company> Companies { get; set; }
  }
}
