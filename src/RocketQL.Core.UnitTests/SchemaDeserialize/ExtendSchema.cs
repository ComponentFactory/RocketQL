namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ExtendSchema : UnitTestBase
{
    [Fact]
    public void DirectiveOnly()
    {
        var documentNode = Serialization.SchemaDeserialize("extend schema @bar");

        var extend = documentNode.NotNull().ExtendSchemas.NotNull().One();
        var directive = extend.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        extend.OperationTypes.NotNull().Count(0);
    }

    [Fact]
    public void OperationTypesOnly()
    {
        var documentNode = Serialization.SchemaDeserialize("extend schema { query: bar }");

        var extend = documentNode.NotNull().ExtendSchemas.NotNull().One();
        extend.Directives.NotNull().Count(0);
        var operation = extend.OperationTypes.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("bar", operation.NamedType);
    }

    [Fact]
    public void DirectiveAndOperationTypes()
    {
        var documentNode = Serialization.SchemaDeserialize("extend schema @bar { query: bar }");

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

    [Fact]
    public void ExtendSchemaMissingAtLeastOne()
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize("extend schema 42");
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
