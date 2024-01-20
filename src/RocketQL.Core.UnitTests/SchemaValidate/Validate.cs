namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Validate : UnitTestBase
{
    [Fact]
    public void OperationNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        schema.AddSyntaxNode(new SyntaxOperationDefinitionNode(OperationType.QUERY, "Example", [], [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => schema.Build());
        Assert.Equal("Operation definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void FragmentNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        schema.AddSyntaxNode(new SyntaxFragmentDefinitionNode("Example", "MyType", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => schema.Build());
        Assert.Equal("Fragment definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }
}

