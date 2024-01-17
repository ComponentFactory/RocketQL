namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendScalar : UnitTestBase
{
    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                extend scalar foo
                """,
                "Scalar 'foo' cannot be extended because it is not defined.",
                "extend scalar foo")]
    [InlineData("""
                type Query { alpha: Int }
                extend scalar foo 
                scalar foo
                """,
                "Scalar 'foo' cannot be extended because it is not defined.",
                "extend scalar foo")]
    [InlineData("""
                type Query { alpha: Int }
                scalar foo                
                extend scalar foo 
                """,
                "Extend scalar 'foo' must specify at least one directive.",
                "extend scalar foo")]
    [InlineData("""
                type Query { alpha: Int }
                scalar foo @specifiedBy(url: "url")
                extend scalar foo @specifiedBy(url: "url")
                """,
                "Directive '@specifiedBy' is not repeatable but has been applied multiple times.",
                "scalar foo, directive @specifiedBy")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Fact]
    public void AddDirectiveToScalar()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   scalar foo
                   extend scalar foo @specifiedBy(url: "url")                 
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var directive = foo.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@specifiedBy", directive.Name);
    }
}

