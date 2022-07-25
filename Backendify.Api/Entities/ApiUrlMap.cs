using System.Collections.Concurrent;

namespace Backendify.Api.Entities
{
  public class ApiUrlMap : ConcurrentDictionary<string, string>
  {
    public ApiUrlMap()
    {
    }

    public ApiUrlMap(IEnumerable<KeyValuePair<string, string>> collection)
      : base(collection)
    {
    }
  }
}
