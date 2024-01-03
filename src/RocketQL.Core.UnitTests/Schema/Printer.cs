namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Printer : UnitTestBase
{
    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaDefault(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = schema.Print();
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaIndent2Spaces(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = schema.Print(new SchemaPrintOptions() { IndentCount = 2 });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaIndent2Tabs(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = schema.Print(new SchemaPrintOptions() { IndentCharacter = PrintIndentCharacter.Tab, IndentCount = 2 });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaNoDescriptions(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = schema.Print(new SchemaPrintOptions() { PrintDescriptions = false });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaShowPredefined(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = schema.Print(new SchemaPrintOptions() { PrintPredefined = true });
        print.MatchSnapshot();
    }
}

