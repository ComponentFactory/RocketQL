namespace RocketQL.Core.UnitTests.GraphQLParser;

public class Files
{
    [Theory]
    [InlineData("github.graphql")]
    [InlineData("introspection.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new Core.GraphQLParser(schema);
        var documentNode = t.Parse();
    }
}



