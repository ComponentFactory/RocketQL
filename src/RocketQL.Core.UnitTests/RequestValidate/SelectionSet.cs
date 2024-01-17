using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class SelectionSet : UnitTestBase
{
    private const string _minimumSchema = "type Query { a: Int }";


    [Theory]
    [InlineData("query { aaa(foo: Int foo: Int) }",
                "Duplicate argument 'foo'.",
                "query (anon), field aaa, argument foo")]
    [InlineData("query { aaa { ccc(foo: Int foo: Int) } }",
                "Duplicate argument 'foo'.",
                "query (anon), field aaa, field ccc, argument foo")]
    public void FieldArguments(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(_minimumSchema, requestText, message, commaPath);
    }
}
