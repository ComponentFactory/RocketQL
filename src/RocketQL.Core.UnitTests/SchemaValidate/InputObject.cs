namespace RocketQL.Core.UnitTests.SchemaValidation;

public class InputObject : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException(
            """
            type Query { alpha: Int } 
            input foo { fizz: Int }
            """,
            "input foo { fizz: Int }",
            "Input object 'foo' is already defined.",
            "input object foo");
    }

    [Theory]
    // At least one field
    [InlineData("""
                type Query { alpha: Int }
                input foo
                """,
                "Input object 'foo' must have at least one input field.",
                "input object foo")]
    [InlineData("""
                type Query { alpha: Int }
                input foo {}
                """,
                "Input object 'foo' must have at least one input field.",
                "input object foo")]
    // Double underscores   
    [InlineData("""
                type Query { alpha: Int }
                input __foo { fizz : Int }
                """,
                "Input object '__foo' not allowed to start with two underscores.",
                "input object __foo")]
    [InlineData("""
                type Query { alpha: Int }
                input foo { __fizz : Int }
                """,
                "Input field '__fizz' not allowed to start with two underscores.",
                "input object foo, input field __fizz")]
    // Undefined types
    [InlineData("""
                type Query { alpha: Int }
                input foo { fizz : Buzz }
                """,
                "Undefined type 'Buzz' on input field 'fizz'.",
                "input object foo, input field fizz")]
    // Type errors
    [InlineData("""
                type Query { alpha: Int }
                input foo { fizz : String! @deprecated }
                """,
                "Cannot use @deprecated directive on non-null input field 'fizz'.",
                "input object foo, input field fizz")]
    [InlineData("""                
                type Query { alpha: Int }
                type foo { fizz : Int }
                input bar { buzz: foo }
                """,
                "Input field 'buzz' is not an input type.",
                "input object bar, input field buzz")]
    [InlineData("""                
                type Query { alpha: Int }
                input foo { fizz: foo! }
                """,
                "Input object 'foo' has circular reference requiring a non-null value.",
                "input object foo")]
    // Directive errors
    [InlineData("""
                type Query { alpha: Int }
                input foo @example { fizz : Int }
                """,
                "Undefined directive '@example' defined on input object.",
                "input object foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                input foo { fizz : Int @example }
                """,
                "Undefined directive '@example' defined on input field.",
                "input object foo, input field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                input foo @example { fizz : Int }                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "input object foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                input foo { fizz : Int @example }                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "input object foo, input field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on INPUT_OBJECT
                input foo @example @example { fizz : Int }                  
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "input object foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on INPUT_FIELD_DEFINITION
                input foo  { fizz : Int @example @example }                  
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "input object foo, input field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on INPUT_OBJECT
                input foo @example { fizz : Int }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "input object foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo  { fizz : Int @example }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "input object foo, input field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on INPUT_OBJECT
                input foo @example { fizz : Int }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "input object foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int @example }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "input object foo, input field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on INPUT_OBJECT
                input foo @example(arg1: 123) { fizz : Int }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "input object foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on INPUT_FIELD_DEFINITION
                input foo { fizz : Int  @example(arg1: 123)}                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "input object foo, input field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on INPUT_OBJECT
                input foo @example(arg1: 123) { fizz : Int }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "input object foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int @example(arg1: 123) }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "input object foo, input field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on INPUT_OBJECT
                input foo @example(arg1: null) { fizz : Int }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "input object foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int  @example(arg1: null) }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "input object foo, input field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on INPUT_OBJECT
                input foo @example(arg1: null) { fizz : Int }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "input object foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int @example(arg1: null) }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "input object foo, input field fizz, directive @example, argument arg1")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Theory]
    [InlineData("""                
                type Query { alpha: Int }
                input foo { fizz: first! }
                input first { buzz: foo! }
                """,
                "Input object 'foo' has circular reference requiring a non-null value.",
                "input object foo",
                "Input object 'first' has circular reference requiring a non-null value.",
                "input object first")]
    [InlineData("""                
                type Query { alpha: Int }
                input foo { fizz: first! }
                input first { buzz: second! }
                input second { buzz: foo! }
                """,
                "Input object 'foo' has circular reference requiring a non-null value.",
                "input object foo",
                "Input object 'first' has circular reference requiring a non-null value.",
                "input object first",
                "Input object 'second' has circular reference requiring a non-null value.",
                "input object second")]
    [InlineData("""                
                type Query { alpha: Int }
                input foo { fizz: first! buzz: second! }
                input first { buzz: second }
                input second { buzz: foo! }                
                """,
                "Input object 'foo' has circular reference requiring a non-null value.",
                "input object foo",
                "Input object 'second' has circular reference requiring a non-null value.",
                "input object second")]
    [InlineData("""                
                type Query { alpha: Int }
                input foo { fizz: first! buzz: second! }
                input first { buzz: second! }
                input second { buzz: third! }                
                input third { buzz: foo! }                
                """,
                "Input object 'foo' has circular reference requiring a non-null value.",
                "input object foo",
                "Input object 'first' has circular reference requiring a non-null value.",
                "input object first",
                "Input object 'second' has circular reference requiring a non-null value.",
                "input object second",
                "Input object 'third' has circular reference requiring a non-null value.",
                "input object third")]
    public void ValidationMultipleExceptions(string schemaText, params string[] messages)
    {
        SchemaValidationMultipleExceptions(schemaText, messages);
    }


    [Theory]
    [InlineData("""                
                type Query { query: Int }
                input foo { fizz: foo }
                """)]
    [InlineData("""                
                type Query { query: Int }
                input foo { fizz: [foo!]! }
                """)]
    [InlineData("""    
                type Query { query: Int }
                input foo { fizz: first! }
                input first { first: foo }
                """)]
    [InlineData("""                
                type Query { query: Int }
                input foo { fizz: first! }
                input first { first: [foo!]! }
                """)]
    [InlineData("""                
                type Query { query: Int }
                input foo { fizz: first! }
                input first { first: second! }
                input second { second: third! }
                input third { third: foo }
                """)]
    [InlineData("""                
                type Query { query: Int }
                input foo { fizz: first! }
                input first { first: [second]! }
                input second { second: [third!] }
                input third { third: foo }
                """)]
    public void ValidCircularReference(string schemaText)
    {
        var schema = SchemaFromString(schemaText);
        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = SchemaFromString(
            """
            type Query { query: Int }
            directive @d1 on INPUT_OBJECT
            directive @d2 on INPUT_FIELD_DEFINITION
            input foo @d1 
            {
                bar: [Int] @d2
            }
            """);

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var d1 = foo.Directives.NotNull().One();
        Assert.Equal("@d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var field = foo.InputFields["bar"];
        Assert.NotNull(field);
        Assert.Equal(foo, field.Parent);
        var d2 = field.Directives.NotNull().One();
        Assert.Equal("@d2", d2.Name);
        Assert.Equal(field, d2.Parent);
    }
}

