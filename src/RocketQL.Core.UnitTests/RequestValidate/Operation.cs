using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Operation : UnitTestBase
{
    private const string _minimumSchema = "type Query { a: Int }";

    [Theory]
    // Single anonymous operation
    [InlineData("{ a } { a }",                                  "Anonymous operation is already defined.")]
    [InlineData("{ a } query { a }",                            "Anonymous operation is already defined.")]
    [InlineData("{ a } query foo { a }",                        "Anonymous operation is already defined.")]
    [InlineData("query { a } query { a }",                      "Anonymous operation is already defined.")]
    [InlineData("query { a } mutation { a }",                   "Anonymous operation is already defined.")]
    [InlineData("query { a } query foo { a }",                  "Anonymous operation is already defined.")]
    // Unique name for operations
    [InlineData("query foo { a } query foo { a }",              "Operation name 'foo' is already defined.")]
    [InlineData("query foo { a } mutation foo { a }",           "Operation name 'foo' is already defined.")]
    public void OperationNames(string requestText, string message)
    {
        RequestSchemaValidationSingleException(_minimumSchema, requestText, message);
    }
}
