using Backendify.Api.Entities;
using Backendify.Api.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Backendify.Api.Services.External.Tests
{
  public class RemoteCompanyServiceTests
  {
    [Fact]
    public async Task GetCompany_WithV1Response_ReturnsParsedCompany()
    {
      // arrange
      var urls = GetUrls();
      var factory = new Mock<IHttpClientFactory>().ReturnsV1Json();
      var service = new RemoteCompanyService(factory.Object, urls, NullLogger<RemoteCompanyService>.Instance);

      // act
      var response = await service.GetCompany("123", "gb");

      // assert
      Assert.NotNull(response);
    }

    [Fact]
    public async Task GetCompany_WithV2Response_ReturnsParsedCompany()
    {
      // arrange
      var urls = GetUrls();
      var factory = new Mock<IHttpClientFactory>().ReturnsV2Json();
      var service = new RemoteCompanyService(factory.Object, urls, NullLogger<RemoteCompanyService>.Instance);

      // act
      var response = await service.GetCompany("123", "gb");

      // assert
      Assert.NotNull(response);
    }

    #region Setup

    private static ApiUrlMap GetUrls() => GetUrls("gb");

    private static ApiUrlMap GetUrls(params string[] countryCodes)
    {
      var urls = countryCodes.Select(x => KeyValuePair.Create(x, $"http://foo.bar/{x}/"));
      return new ApiUrlMap(urls);
    }

    #endregion
  }
}