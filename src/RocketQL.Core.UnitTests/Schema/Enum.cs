namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Enum : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "enum foo { FIRST }";
        SchemaValidationException(schemaText, schemaText, "Enum 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("enum __foo { FIRST }",                         "Enum '__foo' not allowed to start with two underscores.")]
    // Undefined directive
    [InlineData("enum foo @example { FIRST }",                  "Undefined directive 'example' defined on enum 'foo'.")]
    [InlineData("enum foo { FIRST @example }",                  "Undefined directive 'example' defined on enum value 'FIRST' of enum 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddEnum()
    {
        var schema = new Schema();
        schema.Add("enum foo { FIRST SECOND }");
        schema.Validate();

        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.EnumValues.Count);
        Assert.NotNull(foo.EnumValues["FIRST"]);
        Assert.NotNull(foo.EnumValues["SECOND"]);
        Assert.Contains(nameof(AddEnum), foo.Location.Source);
    }
}

