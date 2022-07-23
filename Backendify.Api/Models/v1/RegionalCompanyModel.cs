namespace Backendify.Api.Models.v1
{
  public record RegionalCompanyModel(string Id, string Name, DateTime? ActiveUntil)
  {
    public bool IsActive => this.ActiveUntil is null || this.ActiveUntil >= DateTime.Now;
  }
}