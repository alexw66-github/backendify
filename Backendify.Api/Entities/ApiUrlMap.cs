using System.Collections.Concurrent;

namespace Backendify.Api.Entities
{
  public class ApiUrlMap : ConcurrentDictionary<string, string>
  {
    public bool IsReady => this.Keys.Any();
  }
}
