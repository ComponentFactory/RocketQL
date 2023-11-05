namespace RocketQL.Core.UnitTests.SchemaTests;

public class Enum
{
    private static readonly string _foo =
        """
            "description"
            enum foo {
                FIRST
                SECOND
            }
        """;

    [Fact]
    public void AddEnums()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddEnumTypes(syntaxSchemaNode.EnumTypes);
        schema.Validate();

        Assert.Single(schema.EnumTypes);
        var foo = schema.EnumTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.EnumValues.Count);
        Assert.NotNull(foo.EnumValues["FIRST"]);
        Assert.NotNull(foo.EnumValues["SECOND"]);
        Assert.Contains(nameof(AddEnums), foo.Location.Source);
    }

    [Fact]
    public void AddEnum()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddEnum(syntaxSchemaNode.EnumTypes[0]);
        schema.Validate();

        Assert.Single(schema.EnumTypes);
        var foo = schema.EnumTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.EnumValues.Count);
        Assert.NotNull(foo.EnumValues["FIRST"]);
        Assert.NotNull(foo.EnumValues["SECOND"]);
        Assert.Contains(nameof(AddEnum), foo.Location.Source);
    }

    [Fact]
    public void NameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Merge(_foo);
            schema.Merge(_foo);
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Enum type 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

