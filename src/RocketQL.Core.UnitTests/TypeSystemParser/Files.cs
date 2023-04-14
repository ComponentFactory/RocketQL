namespace RocketQL.Core.UnitTests.TypeSystemParser;

public class Files
{
    [Theory]
    [InlineData("github.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new Core.TypeSystemParser(schema);
        var documentNode = t.Parse();
    }
}



