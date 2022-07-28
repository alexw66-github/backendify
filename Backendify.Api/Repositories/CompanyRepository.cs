using Backendify.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backendify.Api.Repositories
{
  /// <summary>
  /// Database of companies.
  /// </summary>
  /// <seealso cref="DbContext" />
  /// <seealso cref="ICompanyRepository" />
  public class CompanyRepository : DbContext, ICompanyRepository
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyRepository"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public CompanyRepository(DbContextOptions<CompanyRepository> options)
        : base(options)
    { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder
          .UseModel(CompanyRepositoryModel.Instance);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Company>()
          .HasKey(new[] { nameof(Company.Id), nameof(Company.CountryCode) });

      modelBuilder.Entity<Company>().Property(x => x.Id)
        .ValueGeneratedNever();
    }

    public DbSet<Company> Companies { get; set; }
  }
}
