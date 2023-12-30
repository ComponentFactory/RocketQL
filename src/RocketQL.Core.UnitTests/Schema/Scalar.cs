using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Scalar : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "scalar foo";
        SchemaValidationException(schemaText, schemaText, "Scalar 'foo' is already defined.");
    }

    [Theory]
    [InlineData("Int")]
    [InlineData("Float")]
    [InlineData("String")]
    [InlineData("Boolean")]
    [InlineData("ID")]
    public void CannotUsePredefinedName(string scalar)
    {
        SchemaValidationException($"scalar {scalar}", $"Scalar '{scalar}' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("scalar __foo",                                     "Scalar '__foo' not allowed to start with two underscores.")]
    // Directive errors
    [InlineData("scalar foo @example",                              "Undefined directive 'example' defined on scalar 'foo'.")]
    [InlineData("""
                directive @example on ENUM
                scalar foo @example                    
                """,                                                "Directive 'example' is not specified for use on scalar 'foo' location.")]
    [InlineData("""
                directive @example on SCALAR
                scalar foo @example @example                
                """,                                                "Directive 'example' is not repeatable but has been applied multiple times on scalar 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on SCALAR
                scalar foo @example                
                """,                                                "Directive 'example' has mandatory argument 'arg1' missing on scalar 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on SCALAR
                scalar foo @example                
                """,                                                "Directive 'example' has mandatory argument 'arg1' missing on scalar 'foo'.")]
    [InlineData("""
                directive @example on SCALAR
                scalar foo @example(arg1: 123)              
                """,                                                "Directive 'example' does not define argument 'arg1' provided on scalar 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on SCALAR
                scalar foo @example(arg1: 123)              
                """,                                                "Directive 'example' does not define argument 'arg1' provided on scalar 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on SCALAR
                scalar foo @example(arg1: null)              
                """,                                                "Directive 'example' has mandatory argument 'arg1' that is specified as null on scalar 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on SCALAR
                scalar foo @example(arg1: null)              
                """,                                                "Directive 'example' has mandatory argument 'arg1' that is specified as null on scalar 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Theory]
    [InlineData("""
                directive @example on SCALAR
                scalar foo @example                    
                """)]
    [InlineData("""
                directive @example(arg0: Int) on SCALAR
                scalar foo @example                    
                """)]
    [InlineData("""
                directive @example(arg0: Int arg1: String arg2: Float) on SCALAR
                scalar foo @example                    
                """)]
    [InlineData("""
                directive @example(arg0: Int) on SCALAR
                scalar foo @example(arg0: 5)                   
                """)]
    [InlineData("""
                directive @example(arg0: Int) on SCALAR
                scalar foo @example(arg0: null)                   
                """)]
    [InlineData("""
                directive @example(arg0: Int!) on SCALAR
                scalar foo @example(arg0: 5)                   
                """)]
    [InlineData("""
                directive @example(arg0: Int!) repeatable on SCALAR
                scalar foo @example(arg0: 5)                   
                """)]
    [InlineData("""
                directive @example(arg0: Int!) repeatable on SCALAR
                scalar foo @example(arg0: 5) @example(arg0: 6) @example(arg0: 7)               
                """)]
    [InlineData("""
                directive @example(arg0: Int! arg1: String! arg2: Float!) on SCALAR
                scalar foo @example(arg0: 5, arg1: "hello" arg2: 3.14)                   
                """)]
    public void ValidDirectiveUse(string schemaText)
    {
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();

        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void AddScalarType()
    {
        var schema = new Schema();
        schema.Add("scalar foo");
        schema.Validate();

        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddScalarType), foo.Location.Source);
    }
}

