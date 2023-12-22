namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Union
{
    private static readonly string _foo =
        """
            "description"
            union foo = fizz | buzz
        """;

    [Fact]
    public void AddUnionType()
    {
        var schema = new Schema();
        schema.Add(_foo);
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.MemberTypes.Count);
        Assert.NotNull(foo.MemberTypes["fizz"]);
        Assert.NotNull(foo.MemberTypes["buzz"]);
        Assert.Contains(nameof(AddUnionType), foo.Location.Source);
    }

    [Fact]
    public void NodeNameAlreadyDefined()
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
            Assert.Equal($"Union name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

