using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class SelectionSet : UnitTestBase
{
    private const string _minimumSchema = "type Query { a: Int }";


    [Theory]
    [InlineData("query { aaa(foo: Int foo: Int) }",                             "Field 'aaa' has duplicate argument 'foo'.")]
    [InlineData("query { aaa { ccc(foo: Int foo: Int) } }",                     "Field 'ccc' has duplicate argument 'foo'.")]
    public void FieldArguments(string requestText, string message)
    {
        RequestSchemaValidationSingleException(_minimumSchema, requestText, message);
    }
}
