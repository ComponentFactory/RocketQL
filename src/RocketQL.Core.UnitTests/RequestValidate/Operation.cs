using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Operation : UnitTestBase
{
    private static readonly string s_minimumSchema = "type Query { a: Int }";

    [Theory]
    // Single anonymous operation
    [InlineData("{ a } { a }",
                "Anonymous operation is already defined.",
                "query (anon)")]
    [InlineData("{ a } query { a }",
                "Anonymous operation is already defined.",
                "query (anon)")]
    [InlineData("query { a } query { a }",
                "Anonymous operation is already defined.",
                "query (anon)")]
    [InlineData("query { a } mutation { a }",
                "Anonymous operation is already defined.",
                "mutation (anon)")]
    // Anonymous operation cannot be used with named operation
    [InlineData("{ a } query foo { a }",
                "Anonymous operation and named operation both defined.",
                "query foo")]
    [InlineData("query { a } query foo { a }",
                "Anonymous operation and named operation both defined.",
                "query foo")]
    //// Unique name for operations
    [InlineData("query foo { a } query foo { a }",
                "Operation name 'foo' is already defined.",
                "query foo")]
    [InlineData("query foo { a } mutation foo { a }",
                "Operation name 'foo' is already defined.",
                "mutation foo")]
    public void OperationNames(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimumSchema, requestText, message, commaPath);
    }

    [Theory]
    [InlineData("query @foo { a }",
                "Undefined directive '@foo' defined on operation.",
                "query (anon), directive @foo")]
    [InlineData("query foo @foo { a }",
                "Undefined directive '@foo' defined on operation.",
                "query foo, directive @foo")]
    public void OperationDirectives(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimumSchema, requestText, message, commaPath);
    }

    [Theory]
    [InlineData("query($foo: Int $foo: Int) { a }",
                "Duplicate variable '$foo'.",
                "query (anon), variable $foo")]
    [InlineData("query bar($foo: Int $foo: Int) { a }",
                "Duplicate variable '$foo'.",
                "query bar, variable $foo")]
    [InlineData("query bar($a: Int $foo: Int $b: Int $foo: String) { a }",
                "Duplicate variable '$foo'.",
                "query bar, variable $foo")]
    [InlineData("query($foo: Int @bar) { a }",
                "Undefined directive '@bar' defined on variable.",
                "query (anon), variable $foo, directive @bar")]
    [InlineData("query foo ($foo: Int @bar) { a }",
                "Undefined directive '@bar' defined on variable.",
                "query foo, variable $foo, directive @bar")]
    [InlineData("query foo ($foo: Example) { a }",
                "Undefined type 'Example' on variable '$foo'.",
                "query foo, variable $foo")]
    public void OperationParameters(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimumSchema, requestText, message, commaPath);
    }
}
