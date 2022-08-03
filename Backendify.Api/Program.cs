using Backendify.Api.Entities;
using Backendify.Api.Internal;
using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services;
using Backendify.Api.Services.External;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddHealthChecks();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddResponseCaching();
services.AddResponseCompression();
services.AddMemoryCache();
services.AddLogging();
services.AddHttpHeaderLogging();
services.AddHttpForwarding();
services.AddNamedHttpClientWithRetryPolicy("Flakey");

services.AddScoped<ICompanyRepository, CompanyRepository>();
services.AddScoped<IRemoteCompanyService, RemoteCompanyService>();
services.AddScoped<ICompanyService, CompanyService>();
services.AddSingletonUrlsFromArguments(args);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseResponseCaching();
app.UseResponseCompression();
app.ConfigureResponseCachingForQueryParameters(TimeSpan.FromDays(1));

if (app.Environment.IsDevelopment())
{
  app.UseHttpLogging();
  app.ConfigureCachedEntriesForDevelopmentPurposes("123","456");
  
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/status"));

app.MapHealthChecks("/status");

app.MapGet(
  "/company",
  async ([FromQuery(Name = "id")] string id, [FromQuery(Name = "country_iso")] string countryCode, ICompanyService service) =>
  await service.GetCompany(id, countryCode))
  .Produces<CompanyModel>((int)HttpStatusCode.OK, MediaTypeNames.Application.Json)
  .Produces((int)HttpStatusCode.NotFound);

app.Run();