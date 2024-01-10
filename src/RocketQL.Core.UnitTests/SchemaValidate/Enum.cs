namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Enum : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException("type Query { alpha: Int } enum foo { FIRST }", 
                                        "enum foo { FIRST }", 
                                        "Enum 'foo' is already defined.");
    }

    [Theory]
    // At least one field
    [InlineData("""
                type Query { alpha: Int }
                enum foo
                """,                                                    "Enum 'foo' must have at least one enum value.")]
    [InlineData("""
                type Query { alpha: Int }
                enum foo {}
                """,                                                    "Enum 'foo' must have at least one enum value.")]
    // Double underscores
    [InlineData("""
                type Query { alpha: Int }
                enum __foo { FIRST }
                """,                                                    "Enum '__foo' not allowed to start with two underscores.")]
    // Enum values must be unique
    [InlineData("""
                type Query { alpha: Int }
                enum foo { FIRST FIRST }
                """,                                                    "Enum 'foo' has duplicate definition of value 'FIRST'.")]
    // Directive errors
    [InlineData("""
                type Query { alpha: Int }
                enum foo @example { FIRST }
                """,                                                    "Undefined directive 'example' defined on enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                enum foo { FIRST @example }
                """,                                                    "Undefined directive 'example' defined on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on SCALAR
                enum foo @example { FIRST }                   
                """,                                                    "Directive 'example' is not specified for use on enum 'foo' location.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on SCALAR
                enum foo { FIRST @example }                   
                """,                                                    "Directive 'example' is not specified for use on enum value 'FIRST' of enum 'foo' location.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                enum foo @example @example { FIRST }                
                """,                                                    "Directive 'example' is not repeatable but has been applied multiple times on enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM_VALUE
                enum foo { FIRST @example @example }                
                """,                                                    "Directive 'example' is not repeatable but has been applied multiple times on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM
                enum foo @example { FIRST }                
                """,                                                    "Directive 'example' has mandatory argument 'arg1' missing on enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM_VALUE
                enum foo  { FIRST @example }                
                """,                                                    "Directive 'example' has mandatory argument 'arg1' missing on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM
                enum foo @example { FIRST }                
                """,                                                    "Directive 'example' has mandatory argument 'arg1' missing on enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM_VALUE
                enum foo  { FIRST @example }                
                """,                                                    "Directive 'example' has mandatory argument 'arg1' missing on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                enum foo @example(arg1: 123) { FIRST }              
                """,                                                    "Directive 'example' does not define argument 'arg1' provided on enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM_VALUE
                enum foo  { FIRST @example(arg1: 123) }              
                """,                                                    "Directive 'example' does not define argument 'arg1' provided on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg1: 123) { FIRST }              
                """,                                                    "Directive 'example' does not define argument 'arg1' provided on enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo  { FIRST @example(arg1: 123) }              
                """,                                                    "Directive 'example' does not define argument 'arg1' provided on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM
                enum foo @example(arg1: null) { FIRST }               
                """,                                                    "Argument 'arg1' of directive 'example' of enum 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg1: null) }               
                """,                                                    "Argument 'arg1' of directive 'example' of enum 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM
                enum foo @example(arg1: null) { FIRST }                   
                """,                                                    "Argument 'arg1' of directive 'example' of enum 'foo' has a default value incompatible with the type.")]    
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg1: null) }               
                """,                                                    "Argument 'arg1' of directive 'example' of enum 'foo' has a default value incompatible with the type.")]    
    public void ValidationSingleExceptions(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
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
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();

        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }


    [Fact]
    public void ParentLinkage()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { query: Int }
                   directive @d1 on ENUM
                   directive @d2 on ENUM_VALUE
                   enum foo @d1 
                   {
                       BAR @d2
                   }
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var d1 = foo.Directives.NotNull().One();
        Assert.Equal("d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var field = foo.EnumValues["BAR"];
        Assert.NotNull(field);
        Assert.Equal(foo, field.Parent);
        var d2 = field.Directives.NotNull().One();
        Assert.Equal("d2", d2.Name);
        Assert.Equal(field, d2.Parent);
    }
}

