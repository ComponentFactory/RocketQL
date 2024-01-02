namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Printer : UnitTestBase
{
    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchema(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = schema.Print();
        print.MatchSnapshot();
    }
}

