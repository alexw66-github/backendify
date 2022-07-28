using Backendify.Api.Entities;
using Backendify.Api.Repositories;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.EntityFrameworkCore;
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
    public static IServiceCollection AddEntityFrameworkWithInMemoryCaching(this IServiceCollection services)
    {
      services.AddDbContextPool<CompanyRepository>(options =>
      {
        options.UseInMemoryDatabase("cache");
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
      });

      services.AddEFSecondLevelCache(options =>
      {
        options.UseMemoryCacheProvider();
        options.DisableLogging(true);
      });

      return services;
    }

    public static IServiceCollection AddNamedHttpClientWithRetryPolicy(this IServiceCollection services, string name, int intervalMs = 250)
    {
      static IAsyncPolicy<HttpResponseMessage> RetryTwice()
      {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode != HttpStatusCode.NotFound && msg.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * 250));
      }

      services.AddHttpClient(name).AddPolicyHandler(RetryTwice());
      return services;
    }

    public static IServiceCollection AddSingletonUrlsFromArguments(this IServiceCollection services, string[] args)
    {
      return services.AddSingleton<ApiUrlMap>(provider => 
        ArgumentParser.Parse(args, provider.GetRequiredService<ILogger<Program>>()));
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

    public static async Task<WebApplication> ConfigureCachedEntriesForDevelopmentPurposes(this WebApplication app, params Company[] entries)
    {
      using var scope = app.Services.CreateScope();
      var cache = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
      var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

      foreach (var entry in entries)
      {
        cache.Companies.Add(entry);
        logger.LogWarning("Added fake company cache entry [{Id},{CountryCode}] for debugging", entry.Id, entry.CountryCode);
      }

      await cache.SaveChangesAsync();
      return app;
    }
  }
}
