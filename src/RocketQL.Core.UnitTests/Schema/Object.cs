namespace RocketQL.Core.UnitTests.SchemaTests;

public class Object
{
    private static readonly string _foo =
        """
            "description"
            type foo { 
                fizz : Integer
                buzz : String
            }  
        """;

    [Fact]
    public void AddObjectTypes()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddObjectTypes(syntaxSchemaNode.ObjectTypes);
        schema.Validate();

        Assert.Single(schema.ObjectTypes);
        var foo = schema.ObjectTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.Fields.Count);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.NotNull(foo.Fields["buzz"]);
        Assert.Contains(nameof(AddObjectTypes), foo.Location.Source);
    }

    [Fact]
    public void AddObjectType()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddObjectType(syntaxSchemaNode.ObjectTypes[0]);
        schema.Validate();

        Assert.Single(schema.ObjectTypes);
        var foo = schema.ObjectTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.Fields.Count);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.NotNull(foo.Fields["buzz"]);
        Assert.Contains(nameof(AddObjectType), foo.Location.Source);
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
            Assert.Equal($"Object type 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

