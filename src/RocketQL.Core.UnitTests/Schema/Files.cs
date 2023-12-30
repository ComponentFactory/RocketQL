namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Files : UnitTestBase
{
    [Theory]
    [InlineData("github.graphql")]
    public void FileSchema(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
    }
}

