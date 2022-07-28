using Backendify.Api.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backendify.Api.Internal
{
  /// <summary>
  /// Helper for parsing arguments.
  /// </summary>
  public static class ArgumentParser
  {
    /// <summary>
    /// Extracts remote company urls from the argument list.
    /// </summary>
    /// <param name="args">The arguments to parse.</param>
    /// <param name="logger">The optional logger to use.</param>
    /// <returns>Returns the parsed urls.</returns>
    public static ApiUrlMap Parse(string[] args, ILogger? logger = null)
    {
      logger ??= NullLogger.Instance;
      logger.LogInformation("Processing arguments: {@Arguments}", string.Join(' ', args ?? Array.Empty<string>()));

      if (args is null)
      {
        return new ApiUrlMap();
      }

      static KeyValuePair<string, Uri> GetUrl(string value)
      {
        var keyValue = value.Split('=', StringSplitOptions.TrimEntries);
        return KeyValuePair.Create(keyValue[0], new Uri(keyValue[1]));
      };

      var keyValues = args
        .Where(x => x is not null)
        .SelectMany(x => x.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Where(x => x.Contains('='))
        .Select(x => GetUrl(x))
        .ToList();

      return new ApiUrlMap(keyValues);
    }
  }
}
