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

        Assert.Single(schema.Enums);
        var foo = schema.Enums["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddEnums), foo.Location.Source);
    }

    [Fact]
    public void AddEnum()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddEnum(syntaxSchemaNode.EnumTypes[0]);
        schema.Validate();

        Assert.Single(schema.Enums);
        var foo = schema.Enums["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
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
            Assert.Equal($"Enum 'foo' is already defined.", ex.Message);
        }
        catch(Exception ex2)
        {
            Assert.Fail("Wrong exception");
        }
    }
}

