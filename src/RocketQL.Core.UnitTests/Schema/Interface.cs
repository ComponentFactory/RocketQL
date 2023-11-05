namespace RocketQL.Core.UnitTests.SchemaTests;

public class Interface
{
    private static readonly string _foo =
        """
            "description"
            interface foo { 
                fizz : Integer
                buzz : String
            }  
        """;

    [Fact]
    public void AddInterfaceTypes()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddInterfaceTypes(syntaxSchemaNode.InterfaceTypes);
        schema.Validate();

        Assert.Single(schema.Interfaces);
        var foo = schema.Interfaces["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.Fields.Count);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.NotNull(foo.Fields["buzz"]);
        Assert.Contains(nameof(AddInterfaceTypes), foo.Location.Source);
    }

    [Fact]
    public void AddInterfaceType()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddInterfaceType(syntaxSchemaNode.InterfaceTypes[0]);
        schema.Validate();

        Assert.Single(schema.Interfaces);
        var foo = schema.Interfaces["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.Fields.Count);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.NotNull(foo.Fields["buzz"]);
        Assert.Contains(nameof(AddInterfaceType), foo.Location.Source);
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
            Assert.Equal($"Interface 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

