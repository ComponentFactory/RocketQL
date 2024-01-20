namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Files : UnitTestBase
{
    [Theory]
    [InlineData("github-schema.graphql")]
    public void FileSchemaValidate(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var builder = new SchemaBuilder();
        builder.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(builder.Build());
        print.MatchSnapshot();
    }
}

