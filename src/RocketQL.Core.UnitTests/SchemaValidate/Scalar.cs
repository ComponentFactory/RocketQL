﻿namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Scalar : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException(
            """
            type Query { fizz: Int } 
            scalar foo     
            """,
            "scalar foo",
            "Scalar 'foo' is already defined.",
            "scalar foo");
    }

    [Theory]
    [InlineData("Int")]
    [InlineData("Float")]
    [InlineData("String")]
    [InlineData("Boolean")]
    [InlineData("ID")]
    public void CannotUsePredefinedName(string scalar)
    {
        SchemaValidationSingleException("type Query { fizz: Int } scalar " + scalar,
                                        $"Scalar '{scalar}' is already defined.",
                                        $"scalar {scalar}");
    }

    [Theory]
    // Double underscores
    [InlineData("""           
                type Query { fizz: Int }
                scalar __foo
                """,
                "Scalar '__foo' not allowed to start with two underscores.",
                "scalar __foo")]
    // Directive errors
    [InlineData("""
                type Query { fizz: Int }
                scalar foo @example 
                """,
                "Undefined directive '@example' defined on scalar.",
                "scalar foo, directive @example")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example on ENUM
                scalar foo @example                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "scalar foo, directive @example")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example on SCALAR
                scalar foo @example @example                
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "scalar foo, directive @example")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example(arg1: Int!) on SCALAR
                scalar foo @example                
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "scalar foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example(arg0: Int arg1: Int!) on SCALAR
                scalar foo @example                
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "scalar foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example on SCALAR
                scalar foo @example(arg1: 123)              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "scalar foo, directive @example")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example(arg0: Int) on SCALAR
                scalar foo @example(arg1: 123)              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "scalar foo, directive @example")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example(arg1: Int!) on SCALAR
                scalar foo @example(arg1: null)              
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "scalar foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { fizz: Int }
                directive @example(arg0: Int arg1: Int!) on SCALAR
                scalar foo @example(arg1: null)              
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "scalar foo, directive @example, argument arg1")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                directive @example on SCALAR
                scalar foo @example                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on SCALAR
                scalar foo @example                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int arg1: String arg2: Float) on SCALAR
                scalar foo @example                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on SCALAR
                scalar foo @example(arg0: 5)                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on SCALAR
                scalar foo @example(arg0: null)                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) on SCALAR
                scalar foo @example(arg0: 5)                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) repeatable on SCALAR
                scalar foo @example(arg0: 5)                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) repeatable on SCALAR
                scalar foo @example(arg0: 5) @example(arg0: 6) @example(arg0: 7)               
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int! arg1: String! arg2: Float!) on SCALAR
                scalar foo @example(arg0: 5, arg1: "hello" arg2: 3.14)                   
                """)]
    public void ValidDirectiveUse(string schemaText)
    {
        var schema = SchemaFromString(schemaText);
        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void ReferenceCreated()
    {
        var schema = SchemaFromString(
            """
            type Query { query: Int }
            scalar foo
            type bar1 { first: foo }
            type bar2 { first(arg: foo): Int }
            interface bar3 { first: foo }
            interface bar4 { first(arg: foo): Int }
            input bar5 { first: foo }
            directive @bar6(arg: foo) on ENUM
            """);

        var foo = schema.Types["foo"];
        Assert.NotNull(foo);
        foo.References.NotNull().Count(6);
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = SchemaFromString(
            """
            type Query { query: Int }
            scalar foo @specifiedBy(url: "Example")
            """);

        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var directive = foo.Directives.NotNull().One();
        Assert.Equal(foo, directive.Parent);
    }
}

