using Backendify.Api.Entities;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Xml;
using v1Models = Backendify.Api.ProviderModels.v1;
using v2Models = Backendify.Api.ProviderModels.v2;

namespace Backendify.Api.Services.External
{
  /// <summary>
  /// Represents a remote company lookup service.
  /// </summary>
  /// <seealso cref="IRemoteCompanyService" />
  public class RemoteCompanyService : IRemoteCompanyService
  {
    private readonly IHttpClientFactory clientFactory;
    private readonly ApiUrlMap urls;
    private readonly ILogger<RemoteCompanyService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteCompanyService"/> class.
    /// </summary>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="urls">The available remote urls.</param>
    /// <param name="logger">The logger to use.</param>
    public RemoteCompanyService(IHttpClientFactory clientFactory, ApiUrlMap urls, ILogger<RemoteCompanyService> logger)
    {
      this.clientFactory = clientFactory;
      this.urls = urls;
      this.logger = logger;
    }

    /// <summary>
    /// Gets the company with the specified identifier and country code.
    /// </summary>
    /// <param name="id">The company identifier.</param>
    /// <param name="countryCode">The country code.</param>
    /// <returns>Returns the matching company, or <c>null</c>.</returns>
    public async Task<Company?> GetCompany(string id, string countryCode)
    {
      using (logger.BeginScope("Id={id}, CountryCode={countryCode}", id, countryCode))
      {
        if (string.IsNullOrWhiteSpace(id))
        {
          throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
          throw new ArgumentNullException(nameof(countryCode));
        }

        if (!urls.ContainsKey(countryCode))
        {
          logger.LogError("A url for the specified country code does not exist");
          return default;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(urls[countryCode], $"/companies/{id}"));
        request.Headers.Add("Accept", MediaTypeNames.Application.Json);

        logger.LogInformation("Requesting company {Id} with code \"{CountryCode}\" from {Url}", id, countryCode, request.RequestUri);
        logger.LogTrace("{@Request}", request);

        try
        {
          using var client = clientFactory.CreateClient("Flakey");
          using var response = await client.SendAsync(request);

          if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
          {
            logger.LogInformation("Company [{Id},{CountryCode}] does not exist on endpoint {Url}", id, countryCode, request.RequestUri);
            return default;
          }

          response.EnsureSuccessStatusCode();

          if (logger.IsEnabled(LogLevel.Trace))
          {
            logger.LogTrace("{@Response}", response.Content.ReadAsStringAsync());
          }

          if (IsApiVersionOne(response))
          {
            return await this.DecodeCompanyVersion1(id, countryCode, response);
          }
          else
          {
            return await this.DecodeCompanyVersion2(id, countryCode, response);
          }
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Unable to retrieve company [{Id},{CountryCode}] from remote endpoint, due to: {Error}", id, countryCode, ex.Message);
          return default;
        }
      }
    }

    private static bool IsApiVersionOne(HttpResponseMessage response)
    {
      response.Headers.TryGetValues(HeaderNames.ContentType, out IEnumerable<string>? responseHeaders);
      response.Content.Headers.TryGetValues(HeaderNames.ContentType, out IEnumerable<string>? contentHeaders);

      return (responseHeaders ?? Array.Empty<string>())
        .Concat(contentHeaders ?? Array.Empty<string>())
        .Contains(CompanyContentHeaders.Version1);
    }

    private async Task<Company> DecodeCompanyVersion1(string id, string countryCode, HttpResponseMessage response)
    {
      logger.LogInformation("Response is \"application/x-company-v1\"");
      var model = await response.Content.ReadFromJsonAsync<v1Models.CompanyModel>();

      if (model is null)
      {
        throw new InvalidDataException($"Response JSON invalid for identifier \"{id}\" and country code \"{countryCode}\".");
      }

      var entity = new Company(
        id,
        countryCode,
        model.CompanyName,
        null,
        XmlConvert.ToDateTime(model.CreatedOn, XmlDateTimeSerializationMode.Utc),
        !string.IsNullOrWhiteSpace(model.ClosedOn) ? XmlConvert.ToDateTime(model.ClosedOn, XmlDateTimeSerializationMode.Utc) : null);

      return entity;
    }

    private async Task<Company> DecodeCompanyVersion2(string id, string countryCode, HttpResponseMessage response)
    {
      logger.LogInformation("Response is \"application/x-company-v2\" or above");
      var model = await response.Content.ReadFromJsonAsync<v2Models.CompanyModel>();

      if (model is null)
      {
        throw new InvalidDataException($"Response JSON invalid for identifier \"{id}\" and country code \"{countryCode}\".");
      }

      var entity = new Company(
        id,
        countryCode,
        model.CompanyName,
        model.TaxId,
        null,
        !string.IsNullOrWhiteSpace(model.DissolvedOn) ? XmlConvert.ToDateTime(model.DissolvedOn, XmlDateTimeSerializationMode.Utc) : null);

      return entity;
    }

    private static class CompanyContentHeaders
    {
      public const string Version1 = "application/x-company-v1";
      public const string Version2 = "application/x-company-v2";
    }
  }
}
