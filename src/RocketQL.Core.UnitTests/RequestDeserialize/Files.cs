namespace RocketQL.Core.UnitTests.RequestDeserialize;

public class Files : UnitTestBase
{
    [Theory]
    [InlineData("introspection-query.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var documentNode = Serialization.RequestDeserialize(schema);
    }
}



