namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Directive : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "directive @foo on ENUM";
        SchemaValidationException(schemaText, schemaText, "Directive 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("directive @__foo on ENUM",                                     "Directive '__foo' not allowed to start with two underscores.")]
    [InlineData("directive @foo(__arg1: String) on ENUM",                       "Directive 'foo' has argument '__arg1' not allowed to start with two underscores.")]
    // Undefined types
    [InlineData("directive @foo(arg1: Fizz) on ENUM",                           "Undefined type 'Fizz' for argument 'arg1' of directive 'foo'.")]
    // Argument errors
    [InlineData("directive @foo(arg1: String, arg1: String) on ENUM",           "Directive 'foo' has duplicate argument 'arg1'.")]
    [InlineData("""
                type bar { first: Int }
                directive @foo(arg1: bar) on ENUM                
                """,                                                            "Directive 'foo' has argument 'arg1' of type 'bar' that is not an input type.")]
    // Circular reference errors
    [InlineData("directive @foo(arg1: String @foo) on ARGUMENT_DEFINITION",     "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                directive @foo(arg1: Int @bar) on ARGUMENT_DEFINITION                
                directive @bar(arg1: Int @foo) on ARGUMENT_DEFINITION                
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                directive @foo(arg1: Int @bar) on ARGUMENT_DEFINITION                
                directive @bar(arg1: Int @fizz) on ARGUMENT_DEFINITION                
                directive @fizz(arg1: Int @foo) on ARGUMENT_DEFINITION                
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                directive @foo(arg1: bar) on SCALAR                
                scalar bar @foo
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                directive @foo(arg1: bar) on ENUM                
                enum bar @foo { FIRST }
                """,                                                            "Directive 'foo' has circular reference to itself.")]    
    [InlineData("""
                directive @foo(arg1: bar) on ENUM_VALUE                
                enum bar { FIRST @foo }
                """,                                                            "Directive 'foo' has circular reference to itself.")]   
    [InlineData("""
                directive @foo(arg1: bar) on INPUT_OBJECT                
                input bar @foo { fizz: Int  }
                """,                                                            "Directive 'foo' has circular reference to itself.")]      
    [InlineData("""
                directive @foo(arg1: bar) on INPUT_FIELD_DEFINITION                                
                input bar  { fizz: Int @foo }
                """,                                                            "Directive 'foo' has circular reference to itself.")]    
    [InlineData("""
                directive @foo(arg1: bar) on SCALAR                                
                input bar { fizz: buzz }
                scalar buzz @foo
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                directive @foo(arg1: bar) on INPUT_FIELD_DEFINITION                                
                input bar { fizz: buzz }
                input buzz { first: String @foo }
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    [InlineData("""
                directive @foo(arg1: bar) on INPUT_FIELD_DEFINITION                                
                input bar { fizz: Int @buzz }
                directive @buzz(arg1: first) on SCALAR                
                scalar first @foo
                """,                                                            "Directive 'foo' has circular reference to itself.")]
    // Directive errors
    [InlineData("directive @foo(arg1: String @example) on ENUM",                "Undefined directive 'example' defined on argument 'arg1' of directive 'foo'.")]
    [InlineData("""
                directive @example on ENUM
                directive @foo(arg1: String @example) on OBJECT                  
                """,                                                            "Directive 'example' is not specified for use on argument 'arg1' of directive 'foo' location.")]
    [InlineData("""
                directive @example on ARGUMENT_DEFINITION
                directive @foo(arg1: String @example @example) on OBJECT                 
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on argument 'arg1' of directive 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example) on OBJECT                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example) on OBJECT                   
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                directive @example on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: 123)) on OBJECT             
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: 123)) on OBJECT               
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: null)) on OBJECT               
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on argument 'buzz' of directive 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                directive @foo(buzz: Int @example(arg1: null)) on OBJECT                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on argument 'buzz' of directive 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
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
}

