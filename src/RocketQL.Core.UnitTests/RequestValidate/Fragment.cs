using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Fragment : UnitTestBase
{
    private const string _minimalSchema = """
                                          type Query { a: Int }
                                          input typeInput { typeInput: Int }
                                          """;

    [Theory]
    [InlineData("fragment fizz on Any @foo { a }",                                     "Undefined directive '@foo' defined on fragment 'fizz'.")]
    public void FragmentDirectives(string requestText, string message)
    {
        RequestSchemaValidationSingleException(_minimalSchema, requestText, message);
    }

    [Theory]
    [InlineData("fragment fizz on Int { a }",                                           "Fragment 'fizz' cannot be applied to scalar 'Int' only an object, interface or union.")]
    [InlineData("fragment fizz on Boolean { a }",                                       "Fragment 'fizz' cannot be applied to scalar 'Boolean' only an object, interface or union.")]
    [InlineData("fragment fizz on String { a }",                                        "Fragment 'fizz' cannot be applied to scalar 'String' only an object, interface or union.")]
    [InlineData("""
                fragment fizz on typeInput { first }   
                """,                                                                    "Fragment 'fizz' cannot be applied to input object 'typeInput' only an object, interface or union.")]
    public void FragmentTypes(string requestText, string message)
    {
        RequestSchemaValidationSingleException(_minimalSchema, requestText, message);
    }
}
