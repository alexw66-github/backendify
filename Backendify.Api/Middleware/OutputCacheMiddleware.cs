using WebEssentials.AspNetCore.OutputCaching;

namespace Backendify.Api.Middleware
{
  public class OutputCacheMetadata
  {
  }

  public class AddOutputCacheMiddleware
  {
    private readonly RequestDelegate _next;

    public AddOutputCacheMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
      if (httpContext.GetEndpoint()?.Metadata.GetMetadata<OutputCacheMetadata>() is { } outputCacheMetadata)
      {
        httpContext.EnableOutputCaching(TimeSpan.FromDays(1));
      }

      await _next(httpContext);
    }
  }
}
