namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Enum : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "enum foo { FIRST }";
        SchemaValidationException(schemaText, schemaText, "Enum 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("enum __foo { FIRST }",                             "Enum '__foo' not allowed to start with two underscores.")]
    // Enum values must be unque
    [InlineData("enum foo { FIRST FIRST }",                         "Enum 'foo' has duplicate definition of value 'FIRST'.")]
    // Directive errors
    [InlineData("enum foo @example { FIRST }",                      "Undefined directive 'example' defined on enum 'foo'.")]
    [InlineData("enum foo { FIRST @example }",                      "Undefined directive 'example' defined on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example on SCALAR
                enum foo @example { FIRST }                   
                """,                                                "Directive 'example' is not specified for use on enum 'foo' location.")]
    [InlineData("""
                directive @example on SCALAR
                enum foo { FIRST @example }                   
                """,                                                "Directive 'example' is not specified for use on enum value 'FIRST' of enum 'foo' location.")]
    [InlineData("""
                directive @example on ENUM
                enum foo @example @example { FIRST }                
                """,                                                "Directive 'example' is not repeatable but has been applied multiple times on enum 'foo'.")]
    [InlineData("""
                directive @example on ENUM_VALUE
                enum foo { FIRST @example @example }                
                """,                                                "Directive 'example' is not repeatable but has been applied multiple times on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ENUM
                enum foo @example { FIRST }                
                """,                                                "Directive 'example' has mandatory argument 'arg1' missing on enum 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ENUM_VALUE
                enum foo  { FIRST @example }                
                """,                                                "Directive 'example' has mandatory argument 'arg1' missing on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ENUM
                enum foo @example { FIRST }                
                """,                                                "Directive 'example' has mandatory argument 'arg1' missing on enum 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ENUM_VALUE
                enum foo  { FIRST @example }                
                """,                                                "Directive 'example' has mandatory argument 'arg1' missing on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example on ENUM
                enum foo @example(arg1: 123) { FIRST }              
                """,                                                "Directive 'example' does not define argument 'arg1' provided on enum 'foo'.")]
    [InlineData("""
                directive @example on ENUM_VALUE
                enum foo  { FIRST @example(arg1: 123) }              
                """,                                                "Directive 'example' does not define argument 'arg1' provided on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg1: 123) { FIRST }              
                """,                                                "Directive 'example' does not define argument 'arg1' provided on enum 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo  { FIRST @example(arg1: 123) }              
                """,                                                "Directive 'example' does not define argument 'arg1' provided on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ENUM
                enum foo @example(arg1: null) { FIRST }               
                """,                                                "Directive 'example' has mandatory argument 'arg1' that is specified as null on enum 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg1: null) }               
                """,                                                "Directive 'example' has mandatory argument 'arg1' that is specified as null on enum value 'FIRST' of enum 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ENUM
                enum foo @example(arg1: null) { FIRST }               
                """,                                                "Directive 'example' has mandatory argument 'arg1' that is specified as null on enum 'foo'.")]    
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg1: null) }               
                """,                                                "Directive 'example' has mandatory argument 'arg1' that is specified as null on enum value 'FIRST' of enum 'foo'.")]    
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Theory]
    [InlineData("""
                directive @example on ENUM
                enum foo @example { FIRST }                     
                """)]
    [InlineData("""
                directive @example on ENUM_VALUE
                enum foo { FIRST @example }                     
                """)]
    [InlineData("""
                directive @example(arg0: Int) on ENUM
                enum foo @example { FIRST }                    
                """)]
    [InlineData("""
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example }                    
                """)]
    [InlineData("""
                directive @example(arg0: Int arg1: String arg2: Float) on ENUM
                enum foo @example { FIRST }                    
                """)]
    [InlineData("""
                directive @example(arg0: Int arg1: String arg2: Float) on ENUM_VALUE
                enum foo { FIRST @example }                    
                """)]
    [InlineData("""
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg0: 5) { FIRST }                   
                """)]
    [InlineData("""
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) }                   
                """)]
    [InlineData("""
                directive @example(arg0: Int) on ENUM
                enum foo @example(arg0: null) { FIRST }                   
                """)]
    [InlineData("""
                directive @example(arg0: Int) on ENUM_VALUE
                enum foo { FIRST @example(arg0: null) }                   
                """)]
    [InlineData("""
                directive @example(arg0: Int!) on ENUM
                enum foo @example(arg0: 5) { FIRST }                   
                """)]
    [InlineData("""
                directive @example(arg0: Int!) on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) }                   
                """)]
    [InlineData("""
                directive @example(arg0: Int!) repeatable on ENUM
                enum foo @example(arg0: 5) { FIRST }                    
                """)]
    [InlineData("""
                directive @example(arg0: Int!) repeatable on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) }                    
                """)]
    [InlineData("""
                directive @example(arg0: Int!) repeatable on ENUM
                enum foo @example(arg0: 5) @example(arg0: 6) @example(arg0: 7) { FIRST }               
                """)]
    [InlineData("""
                directive @example(arg0: Int!) repeatable on ENUM_VALUE
                enum foo { FIRST @example(arg0: 5) @example(arg0: 6) @example(arg0: 7) }               
                """)]
    [InlineData("""
                directive @example(arg0: Int! arg1: String! arg2: Float!) on ENUM
                enum foo @example(arg0: 5, arg1: "hello" arg2: 3.14) { FIRST }                    
                """)]
    [InlineData("""
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

