using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Operation : UnitTestBase
{
    private const string _minimumSchema = "type Query { a: Int }";

    [Theory]
    // Single anonymous operation
    [InlineData("{ a } { a }",                                              "Anonymous operation is already defined.")]
    [InlineData("{ a } query { a }",                                        "Anonymous operation is already defined.")]
    [InlineData("query { a } query { a }",                                  "Anonymous operation is already defined.")]
    [InlineData("query { a } mutation { a }",                               "Anonymous operation is already defined.")]
    // Anonymous operation cannot be used with named operation
    [InlineData("{ a } query foo { a }",                                    "Anonymous operation and named operation both defined.")]
    [InlineData("query { a } query foo { a }",                              "Anonymous operation and named operation both defined.")]
    // Unique name for operations
    [InlineData("query foo { a } query foo { a }",                          "Operation name 'foo' is already defined.")]
    [InlineData("query foo { a } mutation foo { a }",                       "Operation name 'foo' is already defined.")]
    public void OperationNames(string requestText, string message)
    {
        RequestSchemaValidationSingleException(_minimumSchema, requestText, message);
    }

    [Theory]
    [InlineData("query($foo: Int $foo: Int) { a }",                         "Anonymous query operation has duplicate variable '$foo'.")]
    [InlineData("query bar($foo: Int $foo: Int) { a }",                     "Query operation 'bar' has duplicate variable '$foo'.")]
    [InlineData("query bar($a: Int $foo: Int $b: Int $foo: String) { a }",  "Query operation 'bar' has duplicate variable '$foo'.")]
    public void OperationParameters(string requestText, string message)
    {
        RequestSchemaValidationSingleException(_minimumSchema, requestText, message);
    }
}
