using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Fragment : UnitTestBase
{
    private static readonly string s_minimalSchema = """
                                                     type Query { a: Int }
                                                     input typeInput { typeInput: Int }
                                                     type typeObject1 { typeObject1: Int }
                                                     type typeObject2 { typeObject2: Int }
                                                     """;

    [Theory]
    [InlineData("fragment fizz on typeObject1 @foo { a }",
                "Undefined directive '@foo' defined on fragment.",
                "fragment fizz, directive @foo")]
    [InlineData("fragment fizz on typeObject1 { typeObject1 @foo }",
                "Undefined directive '@foo' defined on field.",
                "fragment fizz, field typeObject1, directive @foo")]
    [InlineData("""
                fragment buzz on typeObject1
                {
                    ...fizz @foo
                }

                fragment fizz on typeObject1 
                { 
                    typeObject1
                }   
                """,
                "Undefined directive '@foo' defined on fragment spread.",
                "fragment buzz, fragment spread fizz, directive @foo")]
    [InlineData("""
                fragment buzz on typeObject1
                {
                    ...on typeObject1 @foo {
                        typeObject1
                    }
                }
                """,
                "Undefined directive '@foo' defined on inline fragment.",
                "fragment buzz, inline fragment typeObject1, directive @foo")]
    [InlineData("""
                fragment buzz on typeObject1
                {
                    ...on typeObject2 {
                        ...on typeObject1 @foo {
                            typeObject1
                        }
                    }
                }
                """,
                "Undefined directive '@foo' defined on inline fragment.",
                "fragment buzz, inline fragment typeObject2, inline fragment typeObject1, directive @foo")]
    public void FragmentDirectives(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimalSchema, requestText, message, commaPath);
    }

    [Theory]
    [InlineData("fragment fizz on Int { a }",
                "Fragment 'fizz' cannot be applied to scalar 'Int' only an object, interface or union.",
                "fragment fizz")]
    [InlineData("fragment fizz on Boolean { a }",
                "Fragment 'fizz' cannot be applied to scalar 'Boolean' only an object, interface or union.",
                "fragment fizz")]
    [InlineData("fragment fizz on String { a }",
                "Fragment 'fizz' cannot be applied to scalar 'String' only an object, interface or union.",
                "fragment fizz")]
    [InlineData("""
                fragment fizz on typeInput { first }   
                """, "Fragment 'fizz' cannot be applied to input object 'typeInput' only an object, interface or union.",
                "fragment fizz")]
    public void FragmentTypes(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimalSchema, requestText, message, commaPath);
    }


    [Theory]
    [InlineData("""
                fragment fizz on typeObject1 
                { 
                    typeObject1 @foo
                }
                """,
                "Undefined directive '@foo' defined on field.",
                "fragment fizz, field typeObject1, directive @foo")]
    [InlineData("""
                fragment fizz on typeObject1 
                { 
                    typeObject1 {
                        typeObject1 @foo
                    }
                }
                """,
                "Undefined directive '@foo' defined on field.",
                "fragment fizz, field typeObject1, field typeObject1, directive @foo")]
    [InlineData("""
                fragment fizz on typeObject1 
                { 
                    ...random
                }
                """,
                "Undefined type 'random' specified for fragment spread within fragment 'fizz'.",
                "fragment fizz, fragment spread random")]
    [InlineData("""
                fragment fizz on typeObject1 
                { 
                    ...on random {
                        field
                    }
                }
                """,
                "Undefined type 'random' specified for inline fragment within fragment 'fizz'.",
                "fragment fizz, inline fragment random")]
    public void FragmentSelectionSet(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_minimalSchema, requestText, message, commaPath);
    }
}
