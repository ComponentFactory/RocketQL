namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Union : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException(
            """
            type Query { first: Int} 
            type fizz { buzz: Int } 
            union foo = fizz
            """,
            "union foo = fizz",
            "Union 'foo' is already defined.",
            "union foo");
    }

    [Theory]
    // Double underscores
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int } 
                union __foo = fizz 
                """,
                "Union '__foo' not allowed to start with two underscores.",
                "union __foo")]
    // Undefined member type
    [InlineData("""
                type Query { first: Int } 
                union foo = fizz     
                """,
                "Undefined member type 'fizz'.",
                "union foo, member type fizz")]
    // Duplicate member type
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                union foo = fizz | fizz    
                """,
                "Duplicate member type 'fizz'.",
                "union foo, member type fizz")]
    // Member type can only be an object type
    [InlineData("""
                type Query { first: Int } 
                scalar fizz 
                union foo = fizz
                """,
                "Cannot reference member type 'fizz' because it is a scalar.",
                "union foo, member type fizz")]
    [InlineData("""
                type Query { first: Int } 
                interface fizz { buzz: Int } 
                union foo = fizz
                """,
                "Cannot reference member type 'fizz' because it is an interface.",
                "union foo, member type fizz")]
    [InlineData("""
                type Query { first: Int } 
                enum fizz { BUZZ } 
                union foo = fizz
                """,
                "Cannot reference member type 'fizz' because it is an enum.",
                "union foo, member type fizz")]
    [InlineData("""
                type Query { first: Int } 
                input fizz { buzz: Int } 
                union foo = fizz
                """,
                "Cannot reference member type 'fizz' because it is an input object.",
                "union foo, member type fizz")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                union foo = fizz
                union buzz = foo
                """,
                "Cannot reference member type 'foo' because it is a union.",
                "union buzz, member type foo")]
    // Directive errors
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                union foo @example = fizz 
                """,
                "Undefined directive '@example' defined on union.",
                "union foo, directive @example")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example on ENUM
                union foo @example = fizz                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "union foo, directive @example")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example on UNION
                union foo @example @example = fizz               
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "union foo, directive @example")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example(arg1: Int!) on UNION
                union foo @example = fizz               
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "union foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example(arg0: Int arg1: Int!) on UNION
                union foo @example = fizz               
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "union foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example on UNION
                union foo @example(arg1: 123) = fizz             
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "union foo, directive @example")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example(arg0: Int) on UNION
                union foo @example(arg1: 123) = fizz              
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "union foo, directive @example")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example(arg1: Int!) on UNION
                union foo @example(arg1: null) = fizz              
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "union foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { first: Int } 
                type fizz { buzz: Int }
                directive @example(arg0: Int arg1: Int!) on UNION
                union foo @example(arg1: null) = fizz              
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "union foo, directive @example, argument arg1")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example on UNION
                union foo @example = fizz                   
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int) on UNION
                union foo @example = fizz                   
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int arg1: String arg2: Float) on UNION
                union foo @example = fizz                   
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int) on UNION
                union foo @example(arg0: 5) = fizz                  
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int) on UNION
                union foo @example(arg0: null) = fizz                  
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int!) on UNION
                union foo @example(arg0: 5) = fizz                   
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int!) repeatable on UNION
                union foo @example(arg0: 5) = fizz                   
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int!) repeatable on UNION
                union foo @example(arg0: 5) @example(arg0: 6) @example(arg0: 7) = fizz                   
                """)]
    [InlineData("""
                type Query { query: Int }
                type fizz { buzz: Int }
                directive @example(arg0: Int! arg1: String! arg2: Float!) on UNION
                union foo @example(arg0: 5, arg1: "hello" arg2: 3.14) = fizz                   
                """)]
    public void ValidDirectiveUse(string schemaText)
    {
        var schema = SchemaFromString(schemaText);
        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void ReferenceCreated()
    {
        var schema = SchemaFromString(
            """
            type Query { query: Int }
            type foo { first: Int }
            union bar = foo
            """);

        var foo = schema.Types["foo"];
        Assert.NotNull(foo);
        var union = schema.Types["bar"];
        Assert.NotNull(union);
        foo.References.NotNull().One();
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = SchemaFromString(
            """
            type Query { query: Int }
            directive @d1 on UNION
            type fizz { b1: Int }
            union foo @d1 = fizz
            """);

        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var d1 = foo.Directives.NotNull().One();
        Assert.Equal("@d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var fizz = foo.MemberTypes["fizz"];
        Assert.NotNull(fizz);
        Assert.Equal(foo, fizz.Parent);
    }
}

