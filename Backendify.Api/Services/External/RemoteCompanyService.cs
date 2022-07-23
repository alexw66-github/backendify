using Backendify.Api.Entities;
using Backendify.Api.Models;
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

    public RemoteCompanyService(IHttpClientFactory clientFactory, ApiUrlMap urls)
    {
      this.clientFactory = clientFactory;
      this.urls = urls;
    }

    public async Task<Company> GetCompany(string id, string countryCode)
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
        throw new ArgumentOutOfRangeException(nameof(countryCode), "Specifiec country code does not exist.");
      }

      var request = new HttpRequestMessage(HttpMethod.Get, urls[countryCode]);
      request.Headers.Add("Accept", MediaTypeNames.Application.Json);

      using var client = clientFactory.CreateClient();
      using var response = await client.SendAsync(request);

      response.EnsureSuccessStatusCode();
      response.Headers.TryGetValues(HeaderNames.ContentType, out IEnumerable<string>? contentHeaders);

      if (contentHeaders is not null &&
        contentHeaders.Contains("application/x-company-v1"))
      {
        var model = await response.Content.ReadFromJsonAsync<v1Models.CompanyModel>();

        if (model is null)
        {
          throw new InvalidDataException($"Response JSON invalid for identifier \"{id}\" and country code \"{countryCode}\".");
        }

        var entity = new Company(
          id,
          countryCode,
          model.CompanyName,
          string.Empty,
          XmlConvert.ToDateTime(model.CreatedOn, XmlDateTimeSerializationMode.Utc),
          model.ClosedOn is not null ? XmlConvert.ToDateTime(model.ClosedOn, XmlDateTimeSerializationMode.Utc) : null);

        return entity;
      }
      else
      {
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
