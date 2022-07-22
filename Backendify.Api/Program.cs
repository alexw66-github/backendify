using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/status", () =>
{
    return Results.Ok("OK");
})
.WithName("GetStatus");

app.MapGet("/company", ([FromQuery(Name = "id")] int id, [FromQuery(Name = "country_iso")] int countryIso) =>
{
    return Results.Ok(new CompanyRecord
    (
      "a12bc",
      "FooBar",
      true,
      DateTime.UtcNow.AddYears(3)
    ));
})
.WithName("GetCompanyById");

app.MapGet("/companies", (HttpContext context) =>
{
    context.Response.Headers.TryAdd(HeaderNames.ContentType, "application/x-company-v1");
    return Results.Ok(new[] {new CompanySummaryV1(
    "",
    DateTime.Today.AddYears(-1).ToShortDateString(),
    DateTime.Today.AddDays(-30).ToShortDateString()
  )});
})
.WithName("GetCompanies");

app.Run();

internal record CompanyRecord(string Id, string Name, bool IsActive, DateTime? ActiveUntil)
{
}

record Something([property: JsonPropertyName("hello")] string world) { }

internal record CompanySummaryV1(
  [property: JsonPropertyName("cn")] string CompanyName,
  [property: JsonPropertyName("created_on")] string CreatedOn,
  [property: JsonPropertyName("closed_on")] string ClosedOn)
{
}

internal record CompanySummaryV2(
  [property: JsonPropertyName("company_name")] string CompanyName,
  [property: JsonPropertyName("tin")] string TaxId,
  [property: JsonPropertyName("dissolved_on")] string ClosedOn)
{
}