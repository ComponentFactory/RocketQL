namespace RocketQL.Core.UnitTests.ExecutableParser;

public class Files
{
    [Theory]
    [InlineData("introspection.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new Core.ExecutableParser(schema);
        var documentNode = t.Parse();
    }
}



