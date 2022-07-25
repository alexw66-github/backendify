using Backendify.Api.Entities;
using Backendify.Api.Repositories;
using Backendify.Api.Services.External;
using Backendify.Api.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Backendify.Api.Services.Tests
{
  public class CompanyServiceTests
  {
    [Fact]
    public async Task GetCompany_WithoutCachedValue_CallsRemoteService()
    {
      // arrange
      var request = (id: "123", countryCode: "gb");
      var cache = GetCache();
      var remoteLookup = GetRemoteLookup();
      var service = new CompanyService(cache.Object, remoteLookup.Object, NullLogger<CompanyService>.Instance);
      
      // act
      await service.GetCompany(request.id, request.countryCode);

      // assert
      remoteLookup
        .Verify(x => x.GetCompany(
          It.Is<string>(id => id == request.id),
          It.Is<string>(countryCode => countryCode == request.countryCode)),
          Times.Once());
    }

    [Fact]
    public async Task GetCompany_WithCachedValue_DoesNotCallRemoteService()
    {
      // arrange
      var request = (id: "123", countryCode: "gb");
      var cachedValue = new Company(request.id, request.countryCode, "FooBar", null, null, null);
      var cache = GetCache(cachedValue);
      var remoteLookup = GetRemoteLookup();
      var service = new CompanyService(cache.Object, remoteLookup.Object, NullLogger<CompanyService>.Instance);
      
      // act
      await service.GetCompany(request.id, request.countryCode);

      // assert
      remoteLookup
        .Verify(x => x.GetCompany(
          It.IsAny<string>(),
          It.IsAny<string>()),
          Times.Never());
    }

    #region Setup

    private static Mock<ICompanyRepository> GetCache(params Company[] values)
    {
      var table = DbHelpers.GetMockDbSet(values);
      var context = new Mock<ICompanyRepository>();

      context
        .Setup(x=> x.Companies)
        .Returns(table.Object);

      return context;
    }

    private static Mock<IRemoteCompanyService> GetRemoteLookup()
    {
      var service = new Mock<IRemoteCompanyService>();

      service
        .Setup(x => x.GetCompany(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync(new Company("123", "gb", "FooBar", "A1B2", DateTime.Today, null));

      return service;
    }

    #endregion
  }
}