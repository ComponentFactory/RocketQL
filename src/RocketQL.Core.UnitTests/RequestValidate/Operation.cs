using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Operation : UnitTestBase
{
    private static readonly string s_minimumSchema = "type Query { a: Int b(c: Int): Int }";

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
    public void OperationVariables(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimumSchema, requestText, message, commaPath);
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = new Schema();
        schema.Add("""
                   directive @foo on QUERY
                   type Query
                   {
                       a(b: Int): Int
                       c: Other
                       d: Other
                   }

                   type Other
                   {
                       e: Int
                   }
                   """);
        schema.Validate();

        var request = new Request();
        request.Add("""
                    query($c: Int) @foo 
                    { 
                        a(b: $c)
                        c {
                            ...Frag
                        }
                        d {
                            ...on Other {
                                e
                            }
                        }
                    }

                    fragment Frag on Other
                    {
                        e
                    }
                    """);
        request.ValidateSchema(schema);

        var operation = request.Operations[""];
        Assert.NotNull(operation);
        var variable = operation.Variables["$c"];
        Assert.NotNull(variable);
        Assert.Equal(operation, variable.Parent);
        Assert.NotNull(variable.Type);
        Assert.Equal(variable, variable.Type.Parent);
        var directive = operation.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal(operation, directive.Parent);
        var fielda = operation.SelectionSet[0];
        Assert.NotNull(fielda);
        Assert.IsType<SelectionField>(fielda);
        Assert.Equal(operation, fielda.Parent);
        var fieldc = operation.SelectionSet[1];
        Assert.NotNull(fieldc);
        var fieldcSet = Assert.IsType<SelectionField>(fieldc);
        Assert.Equal(operation, fieldcSet.Parent);
        var fieldFrag = fieldcSet.SelectionSet[0];
        Assert.NotNull(fieldFrag);
        Assert.IsType<SelectionFragmentSpread>(fieldFrag);
        Assert.Equal(fieldc, fieldFrag.Parent);
        var fieldd = operation.SelectionSet[2];
        Assert.NotNull(fieldd);
        var fielddSet = Assert.IsType<SelectionField>(fieldd);
        Assert.Equal(operation, fielddSet.Parent);
        var fieldOther = fielddSet.SelectionSet[0];
        Assert.NotNull(fieldOther);
        Assert.IsType<SelectionInlineFragment>(fieldOther);
        Assert.Equal(fieldd, fieldOther.Parent);
    }
}
