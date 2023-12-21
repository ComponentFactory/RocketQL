namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Scalar
{
    private static readonly string _foo =
        """
            "description"
            scalar foo
        """;

    [Fact]
    public void AddScalarType()
    {
        var schema = new Schema();
        schema.Add(_foo);
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddScalarType), foo.Location.Source);
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
            Assert.Equal($"Scalar type name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

