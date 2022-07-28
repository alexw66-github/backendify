namespace Backendify.Api.Internal.Tests
{
  public class ArgumentParserTests
  {
    [Fact]
    public void Parse_WithUrls_ExtractsMap()
    {
      // arrange
      var args = new[] { "ru=http://localhost:9001 us=http://localhost:9002" };
      var expected = new Dictionary<string, Uri>
        {
          { "ru", new Uri("http://localhost:9001") },
          { "us", new Uri("http://localhost:9002") }
        };

      // act
      var actual = ArgumentParser.Parse(args);

      // assert
      Assert.Equal(expected, actual);
    }
  }
}