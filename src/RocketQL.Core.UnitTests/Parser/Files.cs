namespace RocketQL.Core.UnitTests.Parser;

public class Files
{
    [Theory]
    [InlineData("github.graphql")]
    [InlineData("introspection.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();
    }
}



