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
    public void AddInputObjectType()
    {
        var schema = new Schema();
        schema.Add(_foo);
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
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
            schema.Add(_foo);
            schema.Add(_foo);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Input object type name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

