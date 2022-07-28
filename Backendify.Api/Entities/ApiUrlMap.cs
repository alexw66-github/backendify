using System.Collections.Concurrent;

namespace Backendify.Api.Entities
{
  /// <summary>
  /// Represents a dictionary of country code to remote URLs.
  /// </summary>
  public class ApiUrlMap : ConcurrentDictionary<string, Uri>
  {
    public ApiUrlMap()
    {
    }

    public ApiUrlMap(IEnumerable<KeyValuePair<string, Uri>> collection)
      : base(collection)
    {
    }
  }
}
