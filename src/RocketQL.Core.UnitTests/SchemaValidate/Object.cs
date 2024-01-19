using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Object : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSingleException("""
                                        type Query { alpha: Int } 
                                        type foo { fizz : Int }
                                        """,
                                        "type foo { fizz : Int }",
                                        "Type 'foo' is already defined.",
                                        "type foo");
    }

    [Theory]
    // At least one field
    [InlineData("""
                type Query { alpha: Int }
                type foo         
                """,
                "Type 'foo' must have at least one field.",
                "type foo")]
    [InlineData("""
                type Query { alpha: Int }
                type foo {} 
                """,
                "Type 'foo' must have at least one field.",
                "type foo")]
    // Double underscores
    [InlineData("""
                type Query { alpha: Int }
                type __foo { fizz : Int } 
                """,
                "Type '__foo' not allowed to start with two underscores.",
                "type __foo")]
    [InlineData("""
                type Query { alpha: Int }
                type foo { __fizz : Int }
                """,
                "Field '__fizz' not allowed to start with two underscores.",
                "type foo, field __fizz")]
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz(__arg1: String): String }
                """,
                "Argument '__arg1' not allowed to start with two underscores.",
                "type foo, field fizz, argument __arg1")]
    // Implements errors
    [InlineData("""
                type Query { alpha: Int }
                type foo implements example { fizz : Int }
                """,
                "Undefined interface 'example' defined on type 'foo'.",
                "type foo, interface example")]
    [InlineData("""
                type Query { alpha: Int }
                type foo implements Int { fizz : Int }
                """,
                "Cannot implement interface 'Int' defined on type 'foo' because it is a 'scalar'.",
                "type foo, interface Int")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { first: Int }
                type foo implements first & first { first : Int }
                """,
                "Duplicate interface 'first'.",
                "type foo, interface first")]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                type foo implements first { first: Int second: Int }
                """,
                "Type 'foo' is missing implements 'second' because it is declared on interface 'first'.",
                "type foo")]
    [InlineData("""
                type Query { alpha: Int }
                scalar example
                type foo implements example { fizz : Int }
                """,
                "Cannot implement interface 'example' defined on type 'foo' because it is a 'scalar'.",
                "type foo, interface example")]
    // Undefined types
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz : Buzz }
                """,
                "Undefined type 'Buzz' on field 'fizz'.",
                "type foo, field fizz")]
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz(arg1: Buzz) : String }
                """,
                "Undefined type 'Buzz' on argument 'arg1'.",
                "type foo, field fizz, argument arg1")]
    // Argument errors
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz(arg1: String, arg1: String): String }
                """,
                "Duplicate argument 'arg1'.",
                "type foo, field fizz, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz(arg1: String! @deprecated): String }
                """,
                "Cannot use @deprecated directive on non-null field 'fizz'.",
                "type foo, field fizz, argument arg1")]
    [InlineData("""                
                type Query { alpha: Int }
                type foo { fizz : Int }
                type bar { buzz(arg1: foo): String }
                """,
                "Argument 'arg1' is not an input type.",
                "type bar, field buzz, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int): Int }
                type foo implements first { bar: Int }
                """,
                "Type 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.",
                "type foo, interface first, field bar, argument args1")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int, args2: Int): Int }
                type foo implements first { bar(args2: Int): Int }
                """,
                "Type 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.",
                "type foo, interface first, field bar, argument args1")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int): Int }
                type foo implements first { bar(args1: String): Int }
                """,
                "Type 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.",
                "type foo, interface first, field bar, argument args1")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int): Int }
                type foo implements first { bar(args1: Int!): Int }
                """,
                "Type 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.",
                "type foo, interface first, field bar, argument args1")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: Int }
                type foo implements first { bar(args1: Int!): Int }
                """,
                "Type 'foo' field 'bar' argument 'args1' cannot be non-null type because not declared on interface 'first'.",
                "type foo, interface first, field bar, argument args1")]
    // Field errors
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz: String fizz:String }
                """,
                "Duplicate field 'fizz'.",
                "type foo, field fizz")]
    [InlineData("""
                type Query { alpha: Int }
                input foo { fizz : Int }
                type bar { buzz: foo }                 
                """,
                "Field 'buzz' is not an output type.",
                "type bar, field buzz")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { first: Int }
                type foo implements first { bar: Int }
                """,
                "Type 'foo' is missing field 'first' declared on interface 'first'.",
                "type foo, interface first, field first")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: Int! }
                type foo implements first { bar: Int }
                """,
                "Type 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.",
                "type foo, interface first, field bar")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: Int }
                type foo implements first { bar: [Int] }
                """,
                "Type 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.",
                "type foo, interface first, field bar")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: [Int] }
                type foo implements first { bar: Int }
                """,
                "Type 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.",
                "type foo, interface first, field bar")]
    [InlineData("""
                type Query { alpha: Int }
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa
                interface first { bar: ab }
                type foo implements first { bar: bbb }
                """,
                "Type 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.",
                "type foo, interface first, field bar")]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface third { second: Int }
                type buzz implements third { second: Int }
                interface first { bar: second }
                type foo implements first { bar: buzz }
                """,
                "Type 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.",
                "type foo, interface first, field bar")]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface third { second: Int }
                interface buzz implements third { second: Int }
                interface first { bar: second }
                type foo implements first { bar: buzz }
                """,
                "Type 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.",
                "type foo, interface first, field bar")]
    // Directive errors
    [InlineData("""
                type Query { alpha: Int }
                type foo @example { fizz : Int }
                """,
                "Undefined directive '@example' defined on type.",
                "type foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz : Int @example }
                """,
                "Undefined directive '@example' defined on field.",
                "type foo, field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                type foo { fizz(arg1: Int @example) : Int }
                """,
                "Undefined directive '@example' defined on argument.",
                "type foo, field fizz, argument arg1, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                type foo @example { fizz : Int }                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "type foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                type foo { fizz : Int @example }                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "type foo, field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                type foo { fizz(arg: Int @example) : Int }                    
                """,
                "Directive '@example' is not specified for use at this location.",
                "type foo, field fizz, argument arg, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on OBJECT
                type foo @example @example { fizz : Int }                  
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "type foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on FIELD_DEFINITION
                type foo  { fizz : Int @example @example }                  
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "type foo, field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example @example) : Int }                  
                """,
                "Directive '@example' is not repeatable but has been applied multiple times.",
                "type foo, field fizz, argument arg, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on OBJECT
                type foo @example { fizz : Int }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "type foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on FIELD_DEFINITION
                type foo  { fizz : Int @example }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "type foo, field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example) : Int }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "type foo, field fizz, argument arg, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on OBJECT
                type foo @example { fizz : Int }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "type foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                type foo { fizz : Int @example }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "type foo, field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example) : Int }                  
                """,
                "Directive '@example' has mandatory argument 'arg1' missing.",
                "type foo, field fizz, argument arg, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on OBJECT
                type foo @example(arg1: 123) { fizz : Int }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "type foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on FIELD_DEFINITION
                type foo { fizz : Int @example(arg1: 123)}                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "type foo, field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example(arg1: 123)) : Int }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "type foo, field fizz, argument arg, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on OBJECT
                type foo @example(arg1: 123) { fizz : Int }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "type foo, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on FIELD_DEFINITION
                type foo { fizz : Int @example(arg1: 123) }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "type foo, field fizz, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int  @example(arg1: 123)) : Int }                
                """,
                "Directive '@example' does not define argument 'arg1'.",
                "type foo, field fizz, argument arg, directive @example")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on OBJECT
                type foo @example(arg1: null) { fizz : Int }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "type foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on FIELD_DEFINITION
                type foo { fizz : Int  @example(arg1: null) }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "type foo, field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """,
               "Default value not compatible with type of argument 'arg1'.",
                "type foo, field fizz, argument arg, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on OBJECT
                type foo @example(arg1: null) { fizz : Int }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "type foo, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                type foo { fizz : Int @example(arg1: null) }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "type foo, field fizz, directive @example, argument arg1")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                type foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """,
                "Default value not compatible with type of argument 'arg1'.",
                "type foo, field fizz, argument arg, directive @example, argument arg1")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                type foo implements first & second { bar: Int first: Int }
                """,
                "Type 'foo' is missing field 'second' declared on interface 'first'.",
                "type foo, interface first, field second",
                "Type 'foo' is missing field 'second' declared on interface 'second'.",
                "type foo, interface second, field second")]
    [InlineData("""
                type Query { alpha: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                type foo implements first & second { first: Int second: Int third: Int }
                """,
                "Type 'foo' is missing implements 'third' because it is declared on interface 'first'.",
                "type foo",
                "Type 'foo' is missing implements 'third' because it is declared on interface 'second'.",
                "type foo")]
    [InlineData("""
                type Query { alpha: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                type foo implements first & second { first: Int second: Int third: Int }
                """,
                "Type 'foo' is missing implements 'third' because it is declared on interface 'first'.",
                "type foo",
                "Type 'foo' is missing implements 'third' because it is declared on interface 'second'.",
                "type foo")]
    public void ValidationMultipleExceptions(string schemaText, params string[] messages)
    {
        SchemaValidationMultipleExceptions(schemaText, messages);
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
    public void ValidTypeCheckOnDefaultValue()
    {
        SchemaValidationNoException("""
                                    type Query { query: Int }
                                    type foo 
                                    {
                                       a(arg: Int = 5): Int
                                       b(arg: [Int] = [1, 2]): Int
                                    }
                                    """);
    }

    [Fact]
    public void InvalidTypeCheckOnDefaultValue()
    {
        SchemaValidationSingleException("""
                                        type Query { query: Int }
                                        type foo 
                                        {
                                            a(arg: Int = 3.13): Int
                                        }
                                        """,
                                        "Default value not compatible with type of argument 'arg'.",
                                        "type foo, field a, argument arg");
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
        Assert.Equal("@d1", d1.Name);
        Assert.Equal(foo, d1.Parent);
        var field = foo.Fields["bar"];
        Assert.NotNull(field);
        Assert.Equal(foo, field.Parent);
        var d2 = field.Directives.NotNull().One();
        Assert.Equal("@d2", d2.Name);
        Assert.Equal(field, d2.Parent);
        var argument = field.Arguments["arg"];
        Assert.NotNull(argument);
        Assert.Equal(field, argument.Parent);
        Assert.Equal(argument, argument.Type.Parent);
        var d3 = argument.Directives.NotNull().One();
        Assert.Equal("@d3", d3.Name);
        Assert.Equal(argument, d3.Parent);
        var first = foo.ImplementsInterfaces["first"];
        Assert.NotNull(first);
        Assert.Equal(foo, first.Parent);
    }
}
