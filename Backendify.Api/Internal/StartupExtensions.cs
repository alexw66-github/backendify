using Backendify.Api.Entities;
using Backendify.Api.Repositories;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace Backendify.Api.Internal
{
  /// <summary>
  /// Helper extensions for application startup.
  /// </summary>
  public static class StartupExtensions
  {
    public static IServiceCollection AddNamedHttpClientWithRetryPolicy(this IServiceCollection services, string name, int intervalMs = 250)
    {
      static IAsyncPolicy<HttpResponseMessage> RetryTwice()
      {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode != HttpStatusCode.NotFound && msg.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 150));
      }

      services
        .AddHttpClient(name, options => options.Timeout = TimeSpan.FromSeconds(12))
        .AddPolicyHandler(RetryTwice());
        
      return services;
    }

    public static IServiceCollection AddSingletonUrlsFromArguments(this IServiceCollection services, string[] args)
    {
      return services.AddSingleton<ApiUrlMap>(provider => 
        ArgumentParser.Parse(args, provider.GetRequiredService<ILogger<Program>>()));
    }

    public static IServiceCollection AddHttpHeaderLogging(this IServiceCollection services)
    {
      return services.AddHttpLogging(options =>
      {
        options.LoggingFields =
          HttpLoggingFields.RequestPropertiesAndHeaders |
          HttpLoggingFields.ResponsePropertiesAndHeaders;
      });
    }

    public static IServiceCollection AddHttpForwarding(this IServiceCollection services)
    {
      return services.Configure<ForwardedHeadersOptions>(options =>
      {
          options.ForwardedHeaders =
              ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
      });
    }

    public static WebApplication ConfigureResponseCachingForQueryParameters(this WebApplication app, TimeSpan cacheInterval)
    {
      app.Use(async (context, next) =>
      {
        context.Response.GetTypedHeaders().CacheControl =
            new CacheControlHeaderValue()
            {
              Public = true,
              MaxAge = cacheInterval
            };

        var cachingFeature = context.Features.Get<IResponseCachingFeature>();

        if (cachingFeature is not null)
        {
          cachingFeature.VaryByQueryKeys = new[] { "*" };
        }

        context.Response.Headers[HeaderNames.Vary] = new string[] { HeaderNames.AcceptEncoding };

        await next();
      });

      return app;
    }

    public static WebApplication ConfigureCachedEntriesForDevelopmentPurposes(this WebApplication app, params string[] names)
    {
      var entities = names
        .Select((x,i)=> new Company(x, "gb", $"FooBar{i}", "99L99999", DateTime.Today.AddYears(-1 * i), null))
        .ToArray();

      return app.ConfigureCachedEntriesForDevelopmentPurposes(entities);
    }

    public static WebApplication ConfigureCachedEntriesForDevelopmentPurposes(this WebApplication app, params Company[] entries)
    {
      using var scope = app.Services.CreateScope();
      var cache = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
      var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

      foreach (var entry in entries)
      {
        cache.TrySaveCompany(entry);
        logger.LogWarning("Added fake company cache entry [{Id},{CountryCode}] for debugging", entry.Id, entry.CountryCode);
      }

      return app;
    }
  }
}
