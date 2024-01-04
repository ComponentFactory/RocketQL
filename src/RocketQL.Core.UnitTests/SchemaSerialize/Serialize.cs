namespace RocketQL.Core.UnitTests.SchemaSerialize;

public class Serialize : UnitTestBase
{
    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaDefault(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = Serialization.SchemaSerialize(schema);
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
        string print = Serialization.SchemaSerialize(schema, new SchemaSerializeOptions() { IndentCount = 2 });
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
        string print = Serialization.SchemaSerialize(schema, new SchemaSerializeOptions() { IndentCharacter = PrintIndentCharacter.Tab, IndentCount = 2 });
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
        string print = Serialization.SchemaSerialize(schema, new SchemaSerializeOptions() { IncludeDescription = false });
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
        string print = Serialization.SchemaSerialize(schema, new SchemaSerializeOptions() { IncludeBuiltIn = true });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-unrooted-schema.graphql")]
    public void FileSchemaExcludeUnrooted(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = Serialization.SchemaSerialize(schema, new SchemaSerializeOptions() { IncludeBuiltIn = true, IncludeUnrooted = false });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-unrooted-schema.graphql")]
    public void FileSchemaIncludeUnrooted(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
        string print = Serialization.SchemaSerialize(schema, new SchemaSerializeOptions() { IncludeBuiltIn = true, IncludeUnrooted = true });
        print.MatchSnapshot();
    }
}

