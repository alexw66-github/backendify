using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;

namespace Backendify.Api.Tests.Helpers
{
  public static class HttpHelpers
  {
    public static Mock<IHttpClientFactory> ReturnsV1Json(this Mock<IHttpClientFactory> mock)
    {
      var handler = new Mock<HttpMessageHandler>();
      var content = new StringContent(v1Response);
      content.Headers.ContentType = new MediaTypeHeaderValue(v1Header);

      var response = new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = content,
      };

      handler
         .Protected()
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
         .ReturnsAsync(response);

      mock
        .Setup(x => x.CreateClient(It.IsAny<string>()))
        .Returns(new HttpClient(handler.Object));

      return mock;
    }

    public static Mock<IHttpClientFactory> ReturnsV2Json(this Mock<IHttpClientFactory> mock)
    {
      var handler = new Mock<HttpMessageHandler>();
      var content = new StringContent(v2Response);
      content.Headers.ContentType = new MediaTypeHeaderValue(v2Header);

      var response = new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = content
      };

      handler
         .Protected()
         .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
         .ReturnsAsync(response);

      mock
        .Setup(x => x.CreateClient(It.IsAny<string>()))
        .Returns(new HttpClient(handler.Object));

      return mock;
    }

    private const string v1Header = "application/x-company-v1";
    private const string v1Response = @"{
  ""cn"": ""FooBar"",
  ""created_on"": ""2021-07-13T15:28:51.818095+00:00"",
  ""closed_on"": ""2021-07-13T15:28:51.818095+00:00""
}";
    private const string v2Header = "application/x-company-v2";
    private const string v2Response = @"{
  ""company_name"": ""FooBar"",
  ""tin"": ""99L99999"",
  ""dissolved_on"": ""2021-07-13T15:28:51.818095+00:00""
}";
  }
}
