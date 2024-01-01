namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Input : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "input foo { fizz: Int }";
        SchemaValidationException(schemaText, schemaText, "Input object 'foo' is already defined.");
    }

    [Theory]
    // At least one field
    [InlineData("input foo",                                                    "Input object 'foo' must have at least one input field.")]
    [InlineData("input foo {}",                                                 "Input object 'foo' must have at least one input field.")]
    // Double underscores   
    [InlineData("input __foo { fizz : Int }",                                   "Input object '__foo' not allowed to start with two underscores.")]
    [InlineData("input foo { __fizz : Int }",                                   "Input object 'foo' has field '__fizz' not allowed to start with two underscores.")]
    // Undefined types
    [InlineData("input foo { fizz : Buzz }",                                    "Undefined type 'Buzz' for input field 'fizz' of input object 'foo'.")]
    // Type errors
    [InlineData("input foo { fizz : String! @deprecated }",                     "Cannot use @deprecated directive on non-null input field 'fizz' of input object 'foo'.")]
    [InlineData("""                
                type foo { fizz : Int }
                input bar { buzz: foo }
                """,                                                            "Input object 'bar' has input field 'buzz' of type 'foo' that is not an input type.")]
    [InlineData("""                
                input foo { fizz: foo! }
                """,                                                            "Input object 'foo' has circular reference requiring a non-null value.")]
    [InlineData("""                
                input foo { fizz: first! }
                input first { buzz: foo! }
                
                """,                                                            "Input object 'foo' has circular reference requiring a non-null value.")]
    [InlineData("""                
                input foo { fizz: first! }
                input first { buzz: second! }
                input second { buzz: foo! }
                """,                                                            "Input object 'foo' has circular reference requiring a non-null value.")]
    [InlineData("""                
                input foo { fizz: first! buzz: second! }
                input first { buzz: second }
                input second { buzz: foo! }                
                """,                                                            "Input object 'foo' has circular reference requiring a non-null value.")]
    [InlineData("""                
                input foo { fizz: first! buzz: second! }
                input first { buzz: second! }
                input second { buzz: third! }                
                input third { buzz: foo! }                
                """,                                                            "Input object 'foo' has circular reference requiring a non-null value.")]
    // Directive errors
    [InlineData("input foo @example { fizz : Int }",                            "Undefined directive 'example' defined on input object 'foo'.")]
    [InlineData("input foo { fizz : Int @example }",                            "Undefined directive 'example' defined on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example on ENUM
                input foo @example { fizz : Int }                    
                """,                                                            "Directive 'example' is not specified for use on input object 'foo' location.")]
    [InlineData("""
                directive @example on ENUM
                input foo { fizz : Int @example }                    
                """,                                                            "Directive 'example' is not specified for use on input field 'fizz' of input object 'foo' location.")]
    [InlineData("""
                directive @example on INPUT_OBJECT
                input foo @example @example { fizz : Int }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on input object 'foo'.")]
    [InlineData("""
                directive @example on INPUT_FIELD_DEFINITION
                input foo  { fizz : Int @example @example }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on INPUT_OBJECT
                input foo @example { fizz : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on input object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo  { fizz : Int @example }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on INPUT_OBJECT
                input foo @example { fizz : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on input object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int @example }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example on INPUT_OBJECT
                input foo @example(arg1: 123) { fizz : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on input object 'foo'.")]
    [InlineData("""
                directive @example on INPUT_FIELD_DEFINITION
                input foo { fizz : Int  @example(arg1: 123)}                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on INPUT_OBJECT
                input foo @example(arg1: 123) { fizz : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on input object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int @example(arg1: 123) }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on INPUT_OBJECT
                input foo @example(arg1: null) { fizz : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on input object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int  @example(arg1: null) }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on input field 'fizz' of input object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on INPUT_OBJECT
                input foo @example(arg1: null) { fizz : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on input object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on INPUT_FIELD_DEFINITION
                input foo { fizz : Int @example(arg1: null) }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on input field 'fizz' of input object 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
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
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { query: Int }
                   directive @d1 on INPUT_OBJECT
                   directive @d2 on INPUT_FIELD_DEFINITION
                   input foo @d1 
                   {
                       bar: [Int] @d2
                   }
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var d1 = foo.Directives.NotNull().One();
        Assert.Equal("d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var field = foo.InputFields["bar"];
        Assert.NotNull(field);
        Assert.Equal(foo, field.Parent);
        var d2 = field.Directives.NotNull().One();
        Assert.Equal("d2", d2.Name);
        Assert.Equal(field, d2.Parent);
    }
}

