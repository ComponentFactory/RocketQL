namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Enum : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException(
            """
            type Query { alpha: Int } 
            enum foo { FIRST } 
            """,
            "enum foo { FIRST }",
            "Enum 'foo' is already defined.",
            "enum foo");
    }

    [Theory]
    // At least one field
    [InlineData("""
                type Query { alpha: Int }
                enum foo
                """,
                "Enum 'foo' must have at least one enum value.",
                "enum foo")]
    [InlineData("""
                type Query { alpha: Int }
                enum foo {}
                """,
                "Enum 'foo' must have at least one enum value.",
                "enum foo")]
    // Double underscores
    [InlineData("""
                type Query { alpha: Int }
                enum __foo { FIRST }
                """,
                "Enum '__foo' not allowed to start with two underscores.",
                "enum __foo")]
    // Enum values must be unique
    [InlineData("""
                type Query { alpha: Int }
                enum foo { FIRST FIRST }
                """,
                "Duplicate enum value 'FIRST'.",
                "enum foo, enum value FIRST")]
    // Directive errors
    [InlineData("""
                type Query { alpha: Int }
                enum foo @example { FIRST }
                """,
                "Undefined directive '@example' defined on enum.",
                "enum foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                enum foo { FIRST @example }
                """,
                "Undefined directive '@example' defined on enum value.",
                "enum foo, enum value FIRST, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on SCALAR
                enum foo @example { FIRST }                   
                """,
                "Directive '@example' is not specified for use at this location.",
                "enum foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on SCALAR
                enum foo { FIRST @example }                   
                """,
                "Directive '@example' is not specified for use at this location.",
                "enum foo, enum value FIRST, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                enum foo @example @example { FIRST }                
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "enum foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM_VALUE
                enum foo { FIRST @example @example }                
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "enum foo, enum value FIRST, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM
                enum foo @example { FIRST }                
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "enum foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example }                
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "enum foo, enum value FIRST, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM
                enum foo @example { FIRST }                
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "enum foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example }                
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "enum foo, enum value FIRST, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                enum foo @example(arg1: 123) { FIRST }              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "enum foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM_VALUE
                enum foo { FIRST @example(arg1: 123) }              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "enum foo, enum value FIRST, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg1: 123) { FIRST }              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "enum foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example(arg1: 123) }              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "enum foo, enum value FIRST, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM
                enum foo @example(arg1: null) { FIRST }               
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "enum foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg1: null) }               
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "enum foo, enum value FIRST, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM
                enum foo @example(arg1: null) { FIRST }                   
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "enum foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg1: null) }               
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "enum foo, enum value FIRST, directive @example, argument arg1")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                directive @example on ENUM
                enum foo @example { FIRST }                     
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example on ENUM_VALUE
                enum foo { FIRST @example }                     
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on ENUM
                enum foo @example { FIRST }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int arg1: String arg2: Float) on ENUM
                enum foo @example { FIRST }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int arg1: String arg2: Float) on ENUM_VALUE
                enum foo { FIRST @example }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg0: 5) { FIRST }                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) }                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg0: null) { FIRST }                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example(arg0: null) }                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) on ENUM
                enum foo @example(arg0: 5) { FIRST }                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) }                   
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) repeatable on ENUM
                enum foo @example(arg0: 5) { FIRST }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) repeatable on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) repeatable on ENUM
                enum foo @example(arg0: 5) @example(arg0: 6) @example(arg0: 7) { FIRST }               
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int!) repeatable on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) @example(arg0: 6) @example(arg0: 7) }               
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int! arg1: String! arg2: Float!) on ENUM
                enum foo @example(arg0: 5, arg1: "hello" arg2: 3.14) { FIRST }                    
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @example(arg0: Int! arg1: String! arg2: Float!) on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5, arg1: "hello" arg2: 3.14) }                    
                """)]
    public void ValidDirectiveUse(string schemaText)
    {
        var schema = SchemaFromString(schemaText);
        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }


    [Fact]
    public void ParentLinkage()
    {
        var schema = SchemaFromString(
            """
            type Query { query: Int }
            directive @d1 on ENUM
            directive @d2 on ENUM_VALUE
            enum foo @d1 
            {
                BAR @d2
            }
            """);

        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var d1 = foo.Directives.NotNull().One();
        Assert.Equal("@d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var field = foo.EnumValues["BAR"];
        Assert.NotNull(field);
        Assert.Equal(foo, field.Parent);
        var d2 = field.Directives.NotNull().One();
        Assert.Equal("@d2", d2.Name);
        Assert.Equal(field, d2.Parent);
    }
}

