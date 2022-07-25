using Backendify.Api.Entities;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Xml;
using v1Models = Backendify.Api.ProviderModels.v1;
using v2Models = Backendify.Api.ProviderModels.v2;

namespace Backendify.Api.Services.External
{
  public class RemoteCompanyService : IRemoteCompanyService
  {
    private readonly IHttpClientFactory clientFactory;
    private readonly ApiUrlMap urls;
    private readonly ILogger<RemoteCompanyService> logger;

    public RemoteCompanyService(IHttpClientFactory clientFactory, ApiUrlMap urls, ILogger<RemoteCompanyService> logger)
    {
      this.clientFactory = clientFactory;
      this.urls = urls;
      this.logger = logger;
    }

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
        
        var request = new HttpRequestMessage(HttpMethod.Get, urls[countryCode]);
        request.Headers.Add("Accept", MediaTypeNames.Application.Json);

        logger.LogDebug("Requesting company {Id} from {Url}", id, urls[countryCode]);
        logger.LogTrace("{@Request}", request);

        using var client = clientFactory.CreateClient();
        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        response.Content.Headers.TryGetValues(HeaderNames.ContentType, out IEnumerable<string>? contentHeaders);

        logger.LogTrace("{@Response}", response);

        if (contentHeaders is not null &&
          contentHeaders.Contains("application/x-company-v1"))
        {
          logger.LogDebug("Response is \"application/x-company-v1\"");
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
            model.ClosedOn is not null ? XmlConvert.ToDateTime(model.ClosedOn, XmlDateTimeSerializationMode.Utc) : null);

          return entity;
        }
        else
        {
          logger.LogDebug("Response is \"application/x-company-v2\" or above");
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
            model.DissolvedOn is not null ? XmlConvert.ToDateTime(model.DissolvedOn, XmlDateTimeSerializationMode.Utc) : null);

          return entity;
        }
      }
    }
  }
}
