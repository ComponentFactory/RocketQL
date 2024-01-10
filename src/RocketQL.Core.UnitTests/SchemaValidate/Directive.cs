namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Directive : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException("type Query { alpha: Int } directive @foo on ENUM", 
                                        "directive @foo on ENUM", 
                                        "Directive 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("""
                type Query { alpha: Int }
                directive @__foo on ENUM
                """,                                                            "Directive '__foo' not allowed to start with two underscores.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @foo(__arg1: String) on ENUM
                """,                                                            "Directive 'foo' has argument '__arg1' not allowed to start with two underscores.")]
    // Undefined types
    [InlineData("""
                type Query { alpha: Int }
                directive @foo(arg1: Fizz) on ENUM
                """,                                                            "Undefined type 'Fizz' for argument 'arg1' of directive 'foo'.")]
    // Argument errors
    [InlineData("""
                type Query { alpha: Int }
                directive @foo(arg1: String, arg1: String) on ENUM
                """,                                                            "Directive 'foo' has duplicate argument 'arg1'.")]
    [InlineData("""
                type Query { alpha: Int }
                type bar { first: Int }
                directive @foo(arg1: bar) on ENUM                
                """,                                                            "Directive 'foo' has argument 'arg1' of type 'bar' that is not an input type.")]
   // Circular reference errors
   [InlineData("""
                type Query { alpha: Int }
                directive @foo(arg1: String @foo) on ARGUMENT_DEFINITION
                """,                                                            "Directive 'foo' has circular reference to itself.")]
   [InlineData("""
                type Query { alpha3: Int }
                directive @foo(arg1: bar) on SCALAR                
                scalar bar @foo
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                type Query { alpha4: Int }
                directive @foo(arg1: bar) on ENUM                
                enum bar @foo { FIRST }
                """,                                                            "Directive 'foo' has circular reference to itself.")]    
    [InlineData("""
                type Query { alpha5: Int }
                directive @foo(arg1: bar) on ENUM_VALUE                
                enum bar { FIRST @foo }
                """,                                                            "Directive 'foo' has circular reference to itself.")]   
    [InlineData("""
                type Query { alpha6: Int }
                directive @foo(arg1: bar) on INPUT_OBJECT                
                input bar @foo { fizz: Int  }
                """,                                                            "Directive 'foo' has circular reference to itself.")]      
    [InlineData("""
                type Query { alpha7: Int }
                directive @foo(arg1: bar) on INPUT_FIELD_DEFINITION                                
                input bar  { fizz: Int @foo }
                """,                                                            "Directive 'foo' has circular reference to itself.")]    
    [InlineData("""
                type Query { alpha8: Int }
                directive @foo(arg1: bar) on SCALAR                                
                input bar { fizz: buzz }
                scalar buzz @foo
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                type Query { alpha9: Int }
                directive @foo(arg1: bar) on INPUT_FIELD_DEFINITION                                
                input bar { fizz: buzz }
                input buzz { first: String @foo }
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    // Directive errors
    [InlineData("""
                type Query { alpha: Int }
                directive @foo(arg1: String @example) on ENUM
                """,                                                            "Undefined directive 'example' defined on argument 'arg1' of directive 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                directive @foo(arg1: String @example) on OBJECT                  
                """,                                                            "Directive 'example' is not specified for use on argument 'arg1' of directive 'foo' location.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ARGUMENT_DEFINITION
                directive @foo(arg1: String @example @example) on OBJECT                 
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on argument 'arg1' of directive 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example) on OBJECT                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example) on OBJECT                   
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: 123)) on OBJECT             
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: 123)) on OBJECT               
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: null)) on OBJECT               
                """,                                                            "Argument 'arg1' of directive 'example' of directive 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: null)) on OBJECT                
                """,                                                            "Argument 'arg1' of directive 'example' of directive 'foo' has a default value incompatible with the type.")]
    public void ValidationSingleExceptions(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
    }

    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                directive @foo(arg1: bar) on SCALAR                                
                input bar { fizz: Int @buzz }
                directive @buzz(arg1: first) on INPUT_FIELD_DEFINITION                
                scalar first @foo
                """,                                                                "Directive 'foo' has circular reference to itself.",
                                                                                    "Directive 'buzz' has circular reference to itself.")]
    [InlineData("""
                type Query { alpha1: Int }
                directive @foo(arg1: Int @bar) on ARGUMENT_DEFINITION                
                directive @bar(arg1: Int @foo) on ARGUMENT_DEFINITION                
                """,                                                                "Directive 'foo' has circular reference to itself.",
                                                                                    "Directive 'bar' has circular reference to itself.")]
    [InlineData("""
                type Query { alpha2: Int }
                directive @foo(arg1: Int @bar) on ARGUMENT_DEFINITION                
                directive @bar(arg1: Int @fizz) on ARGUMENT_DEFINITION                
                directive @fizz(arg1: Int @foo) on ARGUMENT_DEFINITION                
                """,                                                                "Directive 'foo' has circular reference to itself.",
                                                                                    "Directive 'bar' has circular reference to itself.",
                                                                                    "Directive 'fizz' has circular reference to itself.")]
    public void ValidationMultipleExceptions(string schemaText, params string[] messages)
    {
        SchemaValidationMultipleExceptions(schemaText, messages);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                type example { field: String @deprecated }
                """)]
    [InlineData("""
                type Query { query: Int }
                type example { field: String @deprecated(reason: "Example") } 
                """)]
    public void PredefinedDirectives(string schemaText)
    {
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
    }

    [Fact]
    public void ReferenceCreated()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { query: Int }
                   directive @foo on SCALAR
                   scalar bar @foo
                   """);
        schema.Validate();

        var foo = schema.Directives["foo"];
        Assert.NotNull(foo);
        var bar = schema.Types["bar"];
        Assert.NotNull(bar);
        foo.References.NotNull().One();
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { query: Int }
                   directive @foo(arg: [Int] @deprecated(reason: "Example")) on ENUM
                   """);
        schema.Validate();

        var foo = schema.Directives["foo"] as DirectiveDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var argument = foo.Arguments["arg"];
        Assert.Equal(foo, argument.Parent);
        var typeList =  argument.Type as TypeList;
        Assert.NotNull(typeList);
        Assert.Equal(argument, typeList.Parent);
        var typeName = typeList.Type as TypeName;
        Assert.NotNull(typeName);
        Assert.Equal(typeList, typeName.Parent);
        var directive = argument.Directives.NotNull().One();
        Assert.NotNull(directive);
        Assert.Equal(argument, directive.Parent);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                directive @foo(arg: Int = 42) on SCALAR
                """)]
    [InlineData("""
                type Query { query: Int }
                directive @foo(arg: Int) on SCALAR
                scalar example @foo(arg: 42)
                """)]
    public void ValidTypeCheckOnDefaultValue(string schemaText)
    {
        SchemaValidationNoException(schemaText);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                directive @foo(arg: Int = 3.14) on SCALAR
                """,
                                                        "Argument 'arg' of directive 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { query: Int }
                directive @foo(arg: Int) on SCALAR
                scalar example @foo(arg: 3.14)
                """,
                                                        "Argument 'arg' of directive 'foo' of scalar 'example' has a default value incompatible with the type.")]
    public void InvalidTypeCheckOnDefaultValue(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
    }
}

