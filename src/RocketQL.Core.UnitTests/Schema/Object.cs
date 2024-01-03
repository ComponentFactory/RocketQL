namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Object : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "type foo { fizz : Int }";
        SchemaValidationException(schemaText, schemaText, "Object 'foo' is already defined.");
    }

    [Theory]
    // At least one field
    [InlineData("type foo",                                                     "Object 'foo' must have at least one field.")]
    [InlineData("type foo {}",                                                  "Object 'foo' must have at least one field.")]
    // Double underscores
    [InlineData("type __foo { fizz : Int }",                                    "Object '__foo' not allowed to start with two underscores.")]
    [InlineData("type foo { __fizz : Int }",                                    "Object 'foo' has field '__fizz' not allowed to start with two underscores.")]
    [InlineData("type foo { fizz(__arg1: String): String }",                    "Object 'foo' has field 'fizz' with argument '__arg1' not allowed to start with two underscores.")]
    // Implements errors
    [InlineData("type foo implements example { fizz : Int }",                   "Undefined interface 'example' defined on object 'foo'.")]
    [InlineData("type foo implements Int { fizz : Int }",                       "Cannot implement interface 'Int' defined on object 'foo' because it is a 'scalar'.")]
    [InlineData("""
                interface second  { second: Int }
                interface first implements second { first: Int second: Int }
                type foo implements first { bar: Int }
                """,                                                            "Object 'foo' is missing implements 'second' because it is declared on interface 'first'.")]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                type foo implements first & second { bar: Int }
                """,                                                            "Object 'foo' is missing implements 'third' because it is declared on interface 'first'.")]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                type foo implements first & second { bar: Int }
                """,                                                            "Object 'foo' is missing implements 'third' because it is declared on interface 'first'.")]
    [InlineData("""
                scalar example
                type foo implements example { fizz : Int }
                """,                                                            "Cannot implement interface 'example' defined on object 'foo' because it is a 'scalar'.")]
    // Undefined types
    [InlineData("type foo { fizz : Buzz }",                                     "Undefined type 'Buzz' for field 'fizz' of object 'foo'.")]
    [InlineData("type foo { fizz(arg1: Buzz) : String }",                       "Undefined type 'Buzz' for argument 'arg1' of object 'foo'.")]
    // Argument errors
    [InlineData("type foo { fizz(arg1: String, arg1: String): String }",        "Object 'foo' has field 'fizz' with duplicate argument 'arg1'.")]
    [InlineData("type foo { fizz(arg1: String! @deprecated): String }",         "Cannot use @deprecated directive on non-null argument 'arg1' of field 'fizz' of object 'foo'.")]
    [InlineData("""                
                type foo { fizz : Int }
                type bar { buzz(arg1: foo): String }
                """,                                                            "Object 'bar' has field 'buzz' with argument 'arg1' of type 'foo' that is not an input type.")]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                type foo implements first { bar: Int }
                """,                                                            "Object 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.")]
    [InlineData("""
                interface first { bar(args1: Int, args2: Int): Int }
                type foo implements first { bar(args2: Int): Int }
                """,                                                            "Object 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.")]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                type foo implements first { bar(args1: String): Int }
                """,                                                            "Object 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.")]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                type foo implements first { bar(args1: Int!): Int }
                """,                                                            "Object 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.")]
    [InlineData("""
                interface first { bar: Int }
                type foo implements first { bar(args1: Int!): Int }
                """,                                                            "Object 'foo' field 'bar' argument 'args1' cannot be non-null type because not declared on interface 'first'.")]
    // Field errors
    [InlineData("type foo { fizz: String fizz:String }",                        "Object 'foo' has duplicate field 'fizz'.")]
    [InlineData("""
                input foo { fizz : Int }
                type bar { buzz: foo }                 
                """,                                                            "Object 'bar' has field 'buzz' with type 'foo' that is not an output type.")]
    [InlineData("""
                interface first { first: Int }
                type foo implements first { bar: Int }
                """,                                                            "Object 'foo' is missing field 'first' declared on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                type foo implements first & second { bar: Int first: Int }
                """,                                                            "Object 'foo' is missing field 'second' declared on interface 'first'.")]
    [InlineData("""
                interface first { bar: Int! }
                type foo implements first { bar: Int }
                """,                                                            "Object 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface first { bar: Int }
                type foo implements first { bar: [Int] }
                """,                                                            "Object 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface first { bar: [Int] }
                type foo implements first { bar: Int }
                """,                                                            "Object 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa
                interface first { bar: ab }
                type foo implements first { bar: bbb }
                """,                                                            "Object 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface third { second: Int }
                type buzz implements third { second: Int }
                interface first { bar: second }
                type foo implements first { bar: buzz }
                """,                                                            "Object 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface third { second: Int }
                interface buzz implements third { second: Int }
                interface first { bar: second }
                type foo implements first { bar: buzz }
                """,                                                            "Object 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    // Directive errors
    [InlineData("type foo @example { fizz : Int }",                             "Undefined directive 'example' defined on object 'foo'.")]
    [InlineData("type foo { fizz : Int @example }",                             "Undefined directive 'example' defined on field 'fizz' of object 'foo'.")]
    [InlineData("type foo { fizz(arg1: Int @example) : Int }",                  "Undefined directive 'example' defined on argument 'arg1' of object 'foo'.")]
    [InlineData("""
                directive @example on ENUM
                type foo @example { fizz : Int }                    
                """,                                                            "Directive 'example' is not specified for use on object 'foo' location.")]
    [InlineData("""
                directive @example on ENUM
                type foo { fizz : Int @example }                    
                """,                                                            "Directive 'example' is not specified for use on field 'fizz' of object 'foo' location.")]
    [InlineData("""
                directive @example on ENUM
                type foo { fizz(arg: Int @example) : Int }                    
                """,                                                            "Directive 'example' is not specified for use on argument 'arg' of field 'fizz' of object 'foo' location.")]
    [InlineData("""
                directive @example on OBJECT
                type foo @example @example { fizz : Int }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on object 'foo'.")]
    [InlineData("""
                directive @example on FIELD_DEFINITION
                type foo  { fizz : Int @example @example }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example @example) : Int }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on argument 'arg' of field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on OBJECT
                type foo @example { fizz : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on FIELD_DEFINITION
                type foo  { fizz : Int @example }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example) : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'arg' of field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on OBJECT
                type foo @example { fizz : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                type foo { fizz : Int @example }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example) : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'arg' of field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example on OBJECT
                type foo @example(arg1: 123) { fizz : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on object 'foo'.")]
    [InlineData("""
                directive @example on FIELD_DEFINITION
                type foo { fizz : Int  @example(arg1: 123)}                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example(arg1: 123)) : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'arg' of field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on OBJECT
                type foo @example(arg1: 123) { fizz : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on FIELD_DEFINITION
                type foo { fizz : Int @example(arg1: 123) }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int  @example(arg1: 123)) : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'arg' of field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on OBJECT
                type foo @example(arg1: null) { fizz : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on FIELD_DEFINITION
                type foo { fizz : Int  @example(arg1: null) }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on argument 'arg' of field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on OBJECT
                type foo @example(arg1: null) { fizz : Int }                
                """,                                                             "Directive 'example' has mandatory argument 'arg1' that is specified as null on object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                type foo { fizz : Int @example(arg1: null) }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on field 'fizz' of object 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on argument 'arg' of field 'fizz' of object 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Theory]
    [InlineData("""
                type Query { query: Int }
                interface first { first: Int }
                type foo implements first { bar: Int first: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                interface first { first: Int }
                type foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                type foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                type foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                type foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface third { third: Int }
                interface second { second: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                type foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar(args1: Int, args2: String!): Int }
                type foo implements first { bar(args1: Int, args2: String!): Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar(args1: Int): Int }
                type foo implements first { bar(args1: Int, args2: String): Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: Int }
                type foo implements first { bar: Int buzz(args1: Int!): Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: Int }
                type foo implements first { bar: Int! }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: [Int] }
                type foo implements first { bar: [Int]! }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: [Int] }
                type foo implements first { bar: [Int!] }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: [Int] }
                type foo implements first { bar: [Int!]! }
                """)]
    [InlineData("""
                type Query { query: Int }
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa | bbb
                interface first { bar: ab }
                type foo implements first { bar: aaa }
                """)]
    [InlineData("""
                type Query { query: Int }
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa | bbb
                interface first { bar: ab }
                type foo implements first { bar: bbb }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                type buzz implements second { second: Int }
                interface first { bar: second }
                type foo implements first { bar: buzz second: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                interface buzz implements second { second: Int }
                interface first { bar: second }
                type foo implements first { bar: buzz second: Int }
                """)]
    public void ImplementsInterface(string schemaText)
    {
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();

        var foo = schema.Types["foo"] as ObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void ParentLinkage()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { query: Int }
                   directive @d1 on OBJECT
                   directive @d2 on FIELD_DEFINITION
                   directive @d3(fizz: Int) on ARGUMENT_DEFINITION
                   interface first { first: Int }
                   type foo implements first @d1 
                   {
                       bar(arg: Int @d3(fizz: 4)): [Int] @d2
                       first: Int
                   }
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as ObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Null(foo.Parent);
        var d1 = foo.Directives.NotNull().One();
        Assert.Equal("d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var field = foo.Fields["bar"];
        Assert.NotNull(field);
        Assert.Equal(foo, field.Parent);
        var d2 = field.Directives.NotNull().One();
        Assert.Equal("d2", d2.Name);
        Assert.Equal(field, d2.Parent);
        var argument = field.Arguments["arg"];
        Assert.NotNull(argument);
        Assert.Equal(field, argument.Parent);
        Assert.Equal(argument, argument.Type.Parent);
        var d3 = argument.Directives.NotNull().One();
        Assert.Equal("d3", d3.Name);
        Assert.Equal(argument, d3.Parent);
        var first = foo.ImplementsInterfaces["first"];
        Assert.NotNull(first);
        Assert.Equal(foo, first.Parent);
    }
}
