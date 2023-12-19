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
    public void AddObjectType()
    {
        var schema = new Schema();
        schema.Add(_foo);
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as ObjectTypeDefinition;
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
            schema.Add(_foo);
            schema.Add(_foo);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Object type name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

