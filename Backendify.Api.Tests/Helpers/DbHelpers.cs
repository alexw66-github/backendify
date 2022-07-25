using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Backendify.Api.Tests.Helpers
{
  public static class DbHelpers
  {
    public static Mock<DbSet<T>> GetMockDbSet<T>(params T[] values)
      where T : class
    {
      var dbSet = values.BuildMock().BuildMockDbSet();
      return dbSet;
    }
  }
}