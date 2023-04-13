namespace RocketQL.Core.UnitTests.GraphQLParser;

public class ExtendScalarTypeDefinition
{
    [Fact]
    public void Minimum()
    {
        var t = new Core.GraphQLParser("extend scalar foo");
        var documentNode = t.Parse();

        var scalar = documentNode.NotNull().ExtendScalarTypes.NotNull().One();
        Assert.Equal("foo", scalar.Name);
        scalar.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var t = new Core.GraphQLParser("extend scalar foo @bar");
        var documentNode = t.Parse();

        var scalar = documentNode.NotNull().ExtendScalarTypes.NotNull().One();
        Assert.Equal("foo", scalar.Name);
        var directive = scalar.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
    }

    [Theory]
    [InlineData("extend")]
    [InlineData("extend scalar")]
    [InlineData("extend scalar foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        var t = new Core.GraphQLParser(text);
        try
        {
            var documentNode = t.Parse();
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



