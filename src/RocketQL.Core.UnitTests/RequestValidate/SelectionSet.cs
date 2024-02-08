namespace RocketQL.Core.UnitTests.RequestValidation;

public class SelectionSet : UnitTestBase
{
    private static readonly string s_objectSchema =
        """
        type Query 
        { 
            aInt: Int 
            bInt: Int 
            cInt(argInt: Int = 5, argFloat: Float!): Int
            dBoolean: Boolean
        }                     
        """;

    [Theory]
    //// Field not defined
    //[InlineData("query { notDefined }",
    //            "Field 'notDefined' not defined on type 'Query'.",
    //            "query (anon), field notDefined")]
    //[InlineData("query foo { notDefined }",
    //            "Field 'notDefined' not defined on type 'Query'.",
    //            "query foo, field notDefined")]
    //// Field arguments
    //[InlineData("query { aaa(foo: Int foo: Int) }",
    //            "Duplicate argument 'foo'.",
    //            "query (anon), field aaa, argument foo")]
    //[InlineData("query { aaa { ccc(foo: Int foo: Int) } }",
    //            "Duplicate argument 'foo'.",
    //            "query (anon), field aaa, field ccc, argument foo")]
    //[InlineData("query { cInt }",
    //            "Field 'cInt' has mandatory argument 'argFloat' missing.",
    //            "query (anon), field cInt, argument argFloat")]
    //[InlineData("query { cInt(argAny: true, argFloat: 3.14) }",
    //            "Field 'cInt' does not define argument 'argAny'.",
    //            "query (anon), field cInt")]
    //[InlineData("query { cInt(argFloat: true) }",
    //            "Value not compatible with type of argument 'argFloat'.",
    //            "query (anon), field cInt, argument argFloat")]
    [InlineData("query($var: Float) { cInt(argFloat: $var) }",
                "Value not compatible with type of argument 'argFloat'.",
                "query (anon), field cInt, argument argFloat")]
    public void ObjectField(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_objectSchema, requestText, message, commaPath);
    }


    [Theory]
    // Merge
    [InlineData("query { renamed : aInt renamed bInt }",
                "Multiple field selections have same name 'renamed' but incompatible types.",
                "query (anon), field renamed (aInt)")]
    [InlineData("query { renamed : aInt renamed dBoolean }",
                "Multiple field selections have same name 'renamed' but incompatible types.",
                "query (anon), field renamed (aInt)")]
    [InlineData("query { cInt(argInt: 1) cInt(argInt: 2) }",
                "Multiple field selections have same name 'renamed' but incompatible argument values.",
                "query (anon), field renamed (aInt)")]
    [InlineData("query($arg1: Int, $arg2: Int) { cInt(argInt: $arg1) cInt(argInt: $arg2) }",
                "Multiple field selections have same name 'cInt' but incompatible argument values.",
                "query (anon), field renamed (aInt)")]
    public void ObjectFieldInvalid(string requestText, string message, string commaPath)
    {
        RequestValidationSingleException(s_objectSchema, requestText, message, commaPath);
    }

    [Theory]
    // Arguments
    [InlineData("query($var: Float) { cInt(argFloat: $var) }")]
    // Merge
    //[InlineData("query { aInt aInt }")]
    //[InlineData("query { aInt aInt bInt bInt dBoolean dBoolean }")]
    //[InlineData("query { renamed: aInt renamed: aInt }")]
    //[InlineData("query { aaa: aInt aaa: aInt bbb: bInt bbb: bInt ddd: dBoolean ddd: dBoolean }")]
    //[InlineData("query { cInt(argInt: 1) cInt(argInt: 1) }")]
    //[InlineData("query { renamed: cInt(argInt: 1) renamed: cInt(argInt: 1) }")]
    //[InlineData("query($argInt: Int) { cInt(argInt: $argInt) cInt(argInt: $argInt) }")]
    public void ObjectFieldValid(string requestText)
    {
        RequestValidationNoException(s_objectSchema, requestText);
    }
}
