using Backendify.Api.Entities;
using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services;
using Backendify.Api.Services.External;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<CompanyRepository>(opt => opt.UseInMemoryDatabase("cache"));
services.AddHttpClient();
services.AddHealthChecks();

services.AddSingleton<ApiUrlMap>(provider =>
{
  (string CountryCode, string Url) GetUrl(string value)
  {
    var keyValue = value.Split('=');
    return (keyValue[0], keyValue[1]);
  };

  var map = new ApiUrlMap();
  var keyValues = args.Where(x => x is not null && x.Contains('='))
    .SelectMany(x=> x.Split(' ', StringSplitOptions.RemoveEmptyEntries| StringSplitOptions.TrimEntries))
    .Select(x => GetUrl(x))
    .ToList();

  keyValues.ForEach(x => map.AddOrUpdate(x.CountryCode, x.Url, (key, oldValue) => x.Url));

  return map;
});
services.AddScoped<IRemoteCompanyService, RemoteCompanyService>();
services.AddScoped<ICompanyService, CompanyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapHealthChecks("/status");

app.MapGet(
  "/company",
  async ([FromQuery(Name = "id")] string id, [FromQuery(Name = "country_iso")] string countryCode, ICompanyService service) =>
  await service.GetCompany(id, countryCode))
  .Produces<CompanyModel>()
  .Produces((int)HttpStatusCode.NotFound);

app.Run();