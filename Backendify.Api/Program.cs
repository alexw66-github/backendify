using Asp.Versioning.Conventions;
using Backendify.Api;
using Backendify.Api.Data;
using Backendify.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Net.Mime;
using v1Models = Backendify.Api.Models.v1;
using v1Services = Backendify.Api.Services.v1;
using v2Models = Backendify.Api.Models.v2;
using v2Services = Backendify.Api.Services.v2;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services
  .AddApiVersioning(options =>
    {
      options.ReportApiVersions = true;
    })
  .AddApiExplorer(options =>
    {
      options.GroupNameFormat = "'v'VVV";
      options.SubstituteApiVersionInUrl = true;
    });
services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());
services.AddScoped<v1Services.ICompanyService, CompanyService>();
services.AddScoped<v2Services.ICompanyService, CompanyService>();
services.AddScoped<v1Services.IStatusService, StatusService>();

var fakeData = new[]
{
  new Company()
  {
    Id = "1",
    CountryCode = "gb",
    CompanyName = "FooBar",
    TaxId = "A1B2",
    Opened = DateTime.Today.AddYears(-3)
  }
};

services.AddSingleton<ICompanyRepository>(provider => new CompanyRepository(fakeData));

var app = builder.Build();
var statusApi = app.NewApiVersionSet("Status").Build();
var companiesApi = app.NewApiVersionSet("Companies").Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    var descriptions = app.DescribeApiVersions();

    foreach (var description in descriptions)
    {
      var url = $"/swagger/{description.GroupName}/swagger.json";
      var name = description.GroupName.ToUpperInvariant();
      options.SwaggerEndpoint(url, name);
    }
  });
}

app.MapGet(
  "/status",
  (v1Services.IStatusService service) =>
  service.GetStatus())
  .WithApiVersionSet(statusApi)
  .HasApiVersion(1.0);

app.MapGet(
  "/company",
  async ([FromQuery(Name = "id")] string id, [FromQuery(Name = "country_iso")] string countryCode, v1Services.ICompanyService service) =>
  await service.GetCompanyByCountry(id, countryCode))
  .Produces<v1Models.RegionalCompanyModel>()
  .Produces((int)HttpStatusCode.NotFound)
  .WithApiVersionSet(companiesApi)
  .HasApiVersion(1.0);

app.MapGet(
  "/companies/{id}",
  async (string id, HttpContext context, v1Services.ICompanyService service) =>
  await service.GetCompanyById(id))
  .Produces<IEnumerable<v1Models.CompanyResultModel>>(contentType: MediaTypeNames.Application.Json, additionalContentTypes: new[] { "application/x-company-v1" })
  .Produces((int)HttpStatusCode.NotFound)
  .WithApiVersionSet(companiesApi)
  .HasApiVersion(1.0);

app.MapGet(
  "/companies/{id}",
  async (string id, HttpContext context, v2Services.ICompanyService services) =>
  await services.GetCompanyById(id))
  .Produces<IEnumerable<v2Models.CompanyResultModel>>(contentType: MediaTypeNames.Application.Json, additionalContentTypes: new[] { "application/x-company-v2" })
  .Produces((int)HttpStatusCode.NotFound)
  .WithApiVersionSet(companiesApi)
  .HasApiVersion(2.0);

app.Run();