using System.Collections.Concurrent;

namespace Backendify.Api.Entities
{
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
