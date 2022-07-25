using Backendify.Api.Entities;
using Backendify.Api.Middleware;
using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services;
using Backendify.Api.Services.External;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<CompanyRepository>(opt => opt.UseInMemoryDatabase("cache"));
services.AddHttpClient();
services.AddHealthChecks();

services.AddScoped<ICompanyRepository, CompanyRepository>();
services.AddScoped<IRemoteCompanyService, RemoteCompanyService>();
services.AddScoped<ICompanyService, CompanyService>();

services.AddLogging();

services.AddSingleton<ApiUrlMap>(provider =>
{
  static KeyValuePair<string, string> GetUrl(string value)
  {
    var keyValue = value.Split('=');
    return KeyValuePair.Create(keyValue[0], keyValue[1]);
  };

  var keyValues = args.Where(x => x is not null && x.Contains('='))
    .SelectMany(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .Select(x => GetUrl(x))
    .ToList();

  return new ApiUrlMap(keyValues);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
else
{
  app.UseMiddleware<AddCacheHeadersMiddleware>();
}

app.MapHealthChecks("/status");

app.MapGet(
  "/company",
  async ([FromQuery(Name = "id")] string id, [FromQuery(Name = "country_iso")] string countryCode, ICompanyService service) =>
  await service.GetCompany(id, countryCode))
  .Produces<CompanyModel>((int)HttpStatusCode.OK, MediaTypeNames.Application.Json)
  .Produces((int)HttpStatusCode.NotFound)
  .WithMetadata(new CacheResponseMetadata());

app.Run();