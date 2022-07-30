using Backendify.Api.Entities;
using Backendify.Api.Internal;
using Backendify.Api.Middleware;
using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services;
using Backendify.Api.Services.External;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;
using WebEssentials.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddHealthChecks();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddResponseCaching();
services.AddOutputCaching(options =>
{
  options.Profiles["default"] = new OutputCacheProfile
  {
    Duration = 86400
  };
});
services.AddMemoryCache();
services.AddLogging();
services.AddHttpLogging(options =>
{
  options.LoggingFields =
    HttpLoggingFields.RequestPropertiesAndHeaders |
    HttpLoggingFields.ResponsePropertiesAndHeaders;
});

services.AddNamedHttpClientWithRetryPolicy("Flakey");

services.AddScoped<ICompanyRepository, CompanyRepository>();
services.AddScoped<IRemoteCompanyService, RemoteCompanyService>();
services.AddScoped<ICompanyService, CompanyService>();
services.AddSingletonUrlsFromArguments(args);

var app = builder.Build();

app.UseResponseCaching();
app.UseOutputCaching();
app.ConfigureResponseCachingForQueryParameters(TimeSpan.FromDays(1));

if (app.Environment.IsDevelopment())
{
  app.UseHttpLogging();
  app.ConfigureCachedEntriesForDevelopmentPurposes(
    new Company("123", "gb", "FooBar1", "99L99999", DateTime.Today.AddYears(-3), null),
    new Company("456", "fr", "FooBar2", "99L99999", DateTime.Today.AddYears(-3), null));
  
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapHealthChecks("/status");

app.MapGet(
  "/company",
  async ([FromQuery(Name = "id")] string id, [FromQuery(Name = "country_iso")] string countryCode, ICompanyService service) =>
  await service.GetCompany(id, countryCode))
  .Produces<CompanyModel>((int)HttpStatusCode.OK, MediaTypeNames.Application.Json)
  .Produces((int)HttpStatusCode.NotFound)
  .WithMetadata(new OutputCacheMetadata());

app.Run();