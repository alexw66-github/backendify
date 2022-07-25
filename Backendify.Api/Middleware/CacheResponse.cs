namespace Backendify.Api.Middleware
{
  public class CacheResponseMetadata
  {
    public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(5);
  }

  public class AddCacheHeadersMiddleware
  {
    private readonly RequestDelegate _next;

    public AddCacheHeadersMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
      if (httpContext.GetEndpoint()?.Metadata.GetMetadata<CacheResponseMetadata>() is { } metadata)
      {
        if (httpContext.Response.HasStarted)
        {
          throw new InvalidOperationException("Can't mutate response after headers have been sent to client.");
        }

        httpContext.Response.Headers.CacheControl = new[] { "public", $"max-age={metadata.CacheTime.TotalSeconds}" };
      }

      await _next(httpContext);
    }
  }
}
