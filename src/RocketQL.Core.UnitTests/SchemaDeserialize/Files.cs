namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class Files : UnitTestBase
{
    [Theory]
    [InlineData("github-schema.graphql")]
    [InlineData("kitchensink-schema.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        Serialization.SchemaDeserialize(schema);
    }
}

