namespace RocketQL.Core.UnitTests.SchemaTests;

public class Input
{
    private static readonly string _foo =
        """
            "description"
            input foo { 
                fizz : Integer
                buzz : String
            }  
        """;

    [Fact]
    public void AddInputObjectTypes()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddInputObjectTypes(syntaxSchemaNode.InputObjectTypes);
        schema.Validate();

        Assert.Single(schema.InputObjectTypes);
        var foo = schema.InputObjectTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.InputFields.Count);
        Assert.NotNull(foo.InputFields["fizz"]);
        Assert.NotNull(foo.InputFields["buzz"]);
        Assert.Contains(nameof(AddInputObjectTypes), foo.Location.Source);
    }

    [Fact]
    public void AddInputObjectType()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddInputObjectType(syntaxSchemaNode.InputObjectTypes[0]);
        schema.Validate();

        Assert.Single(schema.InputObjectTypes);
        var foo = schema.InputObjectTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.InputFields.Count);
        Assert.NotNull(foo.InputFields["fizz"]);
        Assert.NotNull(foo.InputFields["buzz"]);
        Assert.Contains(nameof(AddInputObjectType), foo.Location.Source);
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
            Assert.Equal($"Input object type 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

