using Backendify.Api.Entities;
using Backendify.Api.Middleware;
using Backendify.Api.Models;
using Backendify.Api.Repositories;
using Backendify.Api.Services;
using Backendify.Api.Services.External;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContextPool<CompanyRepository>(options =>
{
  options.UseInMemoryDatabase("cache");
  options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

services.AddPooledDbContextFactory<CompanyRepository>(options =>
{
  options.UseInMemoryDatabase("cache");
});

services.AddEFSecondLevelCache(options =>
{
  options
    .UseMemoryCacheProvider()
    .DisableLogging(true);
});

services.AddResponseCompression(options =>
{
  options.Providers.Add<BrotliCompressionProvider>();
  options.Providers.Add<GzipCompressionProvider>();
});

services.AddResponseCaching();

services.AddLogging();
services.AddHttpLogging(options =>
{
  options.LoggingFields =
    HttpLoggingFields.RequestPropertiesAndHeaders |
    HttpLoggingFields.ResponsePropertiesAndHeaders;
});

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
  return HttpPolicyExtensions
      .HandleTransientHttpError()
      .OrResult(msg => msg.StatusCode != HttpStatusCode.NotFound && msg.StatusCode != HttpStatusCode.OK)
      .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 250));
}

services.AddHttpClient("Flakey").AddPolicyHandler(GetRetryPolicy());

services.AddScoped<ICompanyRepository, CompanyRepository>();
services.AddScoped<IRemoteCompanyService, RemoteCompanyService>();
services.AddScoped<ICompanyService, CompanyService>();

services.AddHealthChecks();

services.AddSingleton<ApiUrlMap>(provider =>
{
  var logger = provider.GetRequiredService<ILogger<ApiUrlMap>>();
  logger.LogDebug("Processing arguments: {Arguments}", string.Join(' ', args ?? Array.Empty<string>()));

  if (args is null)
  {
    return new ApiUrlMap();
  }

  static KeyValuePair<string, Uri> GetUrl(string value)
  {
    var keyValue = value.Split('=');
    return KeyValuePair.Create(keyValue[0], new Uri(keyValue[1]));
  };

  var keyValues = args.Where(x => x is not null && x.Contains('='))
    .SelectMany(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .Select(x => GetUrl(x))
    .ToList();

  return new ApiUrlMap(keyValues);
});

var app = builder.Build();

app.UseHttpLogging();
app.UseResponseCaching();

app.Use(async (context, next) =>
{
  context.Response.GetTypedHeaders().CacheControl =
      new CacheControlHeaderValue()
      {
        Public = true,
        MaxAge = TimeSpan.FromDays(1)
      };

  var cachingFeature = context.Features.Get<IResponseCachingFeature>();

  if (cachingFeature is not null)
  {
    cachingFeature.VaryByQueryKeys = new[] { "*" };
  }

  context.Response.Headers[HeaderNames.Vary] = new string[] { HeaderNames.AcceptEncoding };

  await next();
});

app.UseResponseCompression();
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
  using var scope = app.Services.CreateScope();
  var cache = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
  var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

  cache.Companies.Add(new Company("123", "gb", "FooBar", "99L99999", DateTime.Today.AddYears(-3), null));
  await cache.SaveChangesAsync();
  logger.LogWarning("Added fake cache entry for debugging");
}

app.MapHealthChecks("/status");

app.MapGet(
  "/company",
  async ([FromQuery(Name = "id")] string id, [FromQuery(Name = "country_iso")] string countryCode, ICompanyService service) =>
  await service.GetCompany(id, countryCode))
  .Produces<CompanyModel>((int)HttpStatusCode.OK, MediaTypeNames.Application.Json)
  .Produces((int)HttpStatusCode.NotFound);

app.Run();