using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class SelectionSet : UnitTestBase
{
    private static readonly string s_minimumSchema = "type Query { a: Int }";


    [Theory]
    [InlineData("query { aaa(foo: Int foo: Int) }",
                "Duplicate argument 'foo'.",
                "query (anon), field aaa, argument foo")]
    [InlineData("query { aaa { ccc(foo: Int foo: Int) } }",
                "Duplicate argument 'foo'.",
                "query (anon), field aaa, field ccc, argument foo")]
    public void FieldArguments(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimumSchema, requestText, message, commaPath);
    }
}
