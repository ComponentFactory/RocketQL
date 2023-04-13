namespace RocketQL.Core.UnitTests.GraphQLParser;

public class ExtendSchema
{
    [Fact]
    public void DirectiveOnly()
    {
        var t = new Core.GraphQLParser("extend schema @bar");
        var documentNode = t.Parse();

        var extend = documentNode.NotNull().ExtendSchemas.NotNull().One();
        var directive = extend.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        extend.OperationTypes.NotNull().Count(0);
    }

    [Fact]
    public void OperationTypesOnly()
    {
        var t = new Core.GraphQLParser("extend schema { query: bar }");
        var documentNode = t.Parse();

        var extend = documentNode.NotNull().ExtendSchemas.NotNull().One();
        extend.Directives.NotNull().Count(0);
        var operation = extend.OperationTypes.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("bar", operation.NamedType);
    }

    [Fact]
    public void DirectiveAndOperationTypes()
    {
        var t = new Core.GraphQLParser("extend schema @bar { query: bar }");
        var documentNode = t.Parse();

        var extend = documentNode.NotNull().ExtendSchemas.NotNull().One();
        var directive = extend.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        var operation = extend.OperationTypes.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("bar", operation.NamedType);
    }

    [Theory]
    [InlineData("extend")]
    [InlineData("extend schema")]
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

    [Fact]
    public void ExtendSchemaMissingAtLeastOne()
    {
        var t = new Core.GraphQLParser("extend schema 42");
        try
        {
            var documentNode = t.Parse();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend scheme must specify at least one directive or operation types.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}
