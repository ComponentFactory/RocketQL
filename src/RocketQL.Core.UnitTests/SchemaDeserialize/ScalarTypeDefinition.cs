namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ScalarTypeDefinition : UnitTestBase
{
    [Fact]
    public void Minimum()
    {
        var documentNode = Serialization.SchemaDeserialize("scalar foo");

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal(string.Empty, scalar.Description);
        Assert.Equal("foo", scalar.Name);
        scalar.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" scalar foo")]
    [InlineData("\"\"\"bar\"\"\" scalar foo")]
    public void Description(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal("bar", scalar.Description);
        Assert.Equal("foo", scalar.Name);
        scalar.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("scalar foo @bar");

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal(string.Empty, scalar.Description);
        Assert.Equal("foo", scalar.Name);
        var directive = scalar.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
    }

    [Fact]
    public void DirectiveWithArgument()
    {
        var documentNode = Serialization.SchemaDeserialize("scalar foo @bar(arg1: 123)");

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal(string.Empty, scalar.Description);
        Assert.Equal("foo", scalar.Name);
        var directive = scalar.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        var argument = directive.Arguments.NotNull().One();
        Assert.Equal("arg1", argument.Name);
    }

    [Fact]
    public void DirectiveWithTowArguments()
    {
        var documentNode = Serialization.SchemaDeserialize("scalar foo @bar(arg1: 123, arg2: true)");

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal(string.Empty, scalar.Description);
        Assert.Equal("foo", scalar.Name);
        var directive = scalar.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        Assert.Equal(2, directive.Arguments.Count);
        var argument1 = directive.Arguments.NotNull()[0];
        Assert.Equal("arg1", argument1.Name);
        var argument2 = directive.Arguments.NotNull()[1];
        Assert.Equal("arg2", argument2.Name);
    }

    [Theory]
    [InlineData("scalar")]
    [InlineData("scalar foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Unexpected end of file encountered.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



