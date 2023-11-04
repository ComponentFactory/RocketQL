namespace RocketQL.Core.UnitTests.SchemaTests;

public class Directive
{
    private static readonly string _foo =
        """
            "description"
            directive @foo on ENUM
        """;

    [Fact]
    public void AddDirectives()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddDirectives(syntaxSchemaNode.Directives);
        schema.Validate();

        Assert.Single(schema.Directives);
        var foo = schema.Directives["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddDirectives), foo.Location.Source);
    }

    [Fact]
    public void AddDirective()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddDirective(syntaxSchemaNode.Directives[0]);
        schema.Validate();

        Assert.Single(schema.Directives);
        var foo = schema.Directives["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddDirective), foo.Location.Source);
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
            Assert.Equal($"Directive 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

