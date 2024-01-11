namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Validate : UnitTestBase
{
    [Fact]
    public void OperationNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Add(new SyntaxOperationDefinitionNode(OperationType.QUERY, "Example", [], [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => schema.Validate());
        Assert.Equal("Operation definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void FragmentNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Add(new SyntaxFragmentDefinitionNode("Example", "MyType", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => schema.Validate());
        Assert.Equal("Fragment definition not allowed in a schema.", exception.Message);
    }
}

