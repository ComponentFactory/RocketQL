namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class Files : UnitTestBase
{
    [Theory]
    [InlineData("github.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var documentNode = Serialization.SchemaDeserialize(schema);
    }
}

