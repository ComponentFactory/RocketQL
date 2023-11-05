namespace RocketQL.Core.UnitTests.SchemaTests;

public class Union
{
    private static readonly string _foo =
        """
            "description"
            union foo = fizz | buzz
        """;

    [Fact]
    public void AddUnionTypes()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddUnionTypes(syntaxSchemaNode.UnionTypes);
        schema.Validate();

        Assert.Single(schema.UnionTypes);
        var foo = schema.UnionTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.MemberTypes.Count);
        Assert.NotNull(foo.MemberTypes["fizz"]);
        Assert.NotNull(foo.MemberTypes["buzz"]);
        Assert.Contains(nameof(AddUnionTypes), foo.Location.Source);
    }

    [Fact]
    public void AddUnionType()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddUnionType(syntaxSchemaNode.UnionTypes[0]);
        schema.Validate();

        Assert.Single(schema.UnionTypes);
        var foo = schema.UnionTypes["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.MemberTypes.Count);
        Assert.NotNull(foo.MemberTypes["fizz"]);
        Assert.NotNull(foo.MemberTypes["buzz"]);
        Assert.Contains(nameof(AddUnionType), foo.Location.Source);
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
            Assert.Equal($"Union type 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

