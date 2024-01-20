namespace RocketQL.Core.UnitTests.SchemaSerialize;

public class Serialize : UnitTestBase
{
    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaDefault(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build());
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaIndent2Spaces(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build(), new SchemaSerializeOptions() { IndentCount = 2 });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaIndent2Tabs(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build(), new SchemaSerializeOptions() { IndentCharacter = IndentCharacter.Tab, IndentCount = 2 });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaNoDescriptions(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build(), new SchemaSerializeOptions() { IncludeDescription = false });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-schema.graphql")]
    public void FileSchemaShowPredefined(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build(), new SchemaSerializeOptions() { IncludeBuiltIn = true });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-unrooted-schema.graphql")]
    public void FileSchemaExcludeUnrooted(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build(), new SchemaSerializeOptions() { IncludeBuiltIn = true, IncludeUnrooted = false });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-unrooted-schema.graphql")]
    public void FileSchemaIncludeUnrooted(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build(), new SchemaSerializeOptions() { IncludeBuiltIn = true, IncludeUnrooted = true });
        print.MatchSnapshot();
    }

    [Theory]
    [InlineData("printer-extended-schema.graphql")]
    public void FileSchemaExtended(string filename)
    {
        var schemaText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaText);
        var print = Serialization.SchemaSerialize(schema.Build());
        print.MatchSnapshot();
    }
}

