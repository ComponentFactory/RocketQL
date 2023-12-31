﻿namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Interface : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "interface foo { fizz : Int }";
        SchemaValidationException(schemaText, schemaText, "Interface 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("interface __foo { fizz : Int }",                               "Interface '__foo' not allowed to start with two underscores.")]
    [InlineData("interface foo { __fizz : Int }",                               "Interface 'foo' has field '__fizz' not allowed to start with two underscores.")]
    [InlineData("interface foo { fizz(__arg1: String): String }",               "Interface 'foo' has field 'fizz' with argument '__arg1' not allowed to start with two underscores.")]
    // Implements errors
    [InlineData("interface foo implements example { fizz : Int }",              "Undefined interface 'example' defined on interface 'foo'.")]
    [InlineData("interface foo implements Int { fizz : Int }",                  "Cannot implement interface 'Int' defined on interface 'foo' because it is a 'scalar'.")]
    [InlineData("""
                interface second  { second: Int }
                interface first implements second { first: Int second: Int }
                interface foo implements first { bar: Int }
                """,                                                            "Interface 'foo' is missing implements 'second' because it is declared on interface 'first'.")]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                interface foo implements first & second { bar: Int }
                """,                                                            "Interface 'foo' is missing implements 'third' because it is declared on interface 'first'.")]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                interface foo implements first & second { bar: Int }
                """,                                                            "Interface 'foo' is missing implements 'third' because it is declared on interface 'first'.")]
    [InlineData("""
                scalar example
                interface foo implements example { fizz : Int }
                """,                                                            "Cannot implement interface 'example' defined on interface 'foo' because it is a 'scalar'.")]
    // Undefined types
    [InlineData("interface foo { fizz : Buzz }",                                "Undefined type 'Buzz' for field 'fizz' of interface 'foo'.")]
    [InlineData("interface foo { fizz(arg1: Buzz) : String }",                  "Undefined type 'Buzz' for argument 'arg1' of interface 'foo'.")]
    // Argument errors
    [InlineData("interface foo { fizz(arg1: String, arg1: String): String }",   "Interface 'foo' has field 'fizz' with duplicate argument 'arg1'.")]
    [InlineData("""                
                interface foo { fizz : Int }
                interface bar { buzz(arg1: foo): String }
                """,                                                            "Interface 'bar' has field 'buzz' with argument 'arg1' of type 'foo' that is not an input type.")]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar: Int }
                """,                                                            "Interface 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.")]
    [InlineData("""
                interface first { bar(args1: Int, args2: Int): Int }
                interface foo implements first { bar(args2: Int): Int }
                """,                                                            "Interface 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.")]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar(args1: String): Int }
                """,                                                            "Interface 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.")]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar(args1: Int!): Int }
                """,                                                            "Interface 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.")]
    [InlineData("""
                interface first { bar: Int }
                interface foo implements first { bar(args1: Int!): Int }
                """,                                                            "Interface 'foo' field 'bar' argument 'args1' cannot be non-null type because not declared on interface 'first'.")]
    // Field errors
    [InlineData("interface foo { fizz: String fizz:String }",                   "Interface 'foo' has duplicate field 'fizz'.")]
    [InlineData("""
                input foo { fizz : Int }
                interface bar { buzz: foo }                 
                """,                                                            "Interface 'bar' has field 'buzz' with type 'foo' that is not an output type.")]
    [InlineData("""
                interface first { first: Int }
                interface foo implements first { bar: Int }
                """,                                                            "Interface 'foo' is missing field 'first' declared on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                interface foo implements first & second { bar: Int first: Int }
                """,                                                            "Interface 'foo' is missing field 'second' declared on interface 'first'.")]
    [InlineData("""
                interface first { bar: Int! }
                interface foo implements first { bar: Int }
                """,                                                            "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface first { bar: Int }
                interface foo implements first { bar: [Int] }
                """,                                                            "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface first { bar: [Int] }
                interface foo implements first { bar: Int }
                """,                                                            "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa
                interface first { bar: ab }
                interface foo implements first { bar: bbb }
                """,                                                            "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface third { second: Int }
                type buzz implements third { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz }
                """,                                                            "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface third { second: Int }
                interface buzz implements third { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz }
                """,                                                            "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    // Directive errors
    [InlineData("interface foo @example { fizz : Int }",                        "Undefined directive 'example' defined on interface 'foo'.")]
    [InlineData("interface foo { fizz : Int @example }",                        "Undefined directive 'example' defined on field 'fizz' of interface 'foo'.")]
    [InlineData("interface foo { fizz(arg1: Int @example) : Int }",             "Undefined directive 'example' defined on argument 'arg1' of interface 'foo'.")]
    [InlineData("""
                directive @example on ENUM
                interface foo @example { fizz : Int }                    
                """,                                                            "Directive 'example' is not specified for use on interface 'foo' location.")]
    [InlineData("""
                directive @example on ENUM
                interface foo { fizz : Int @example }                    
                """,                                                            "Directive 'example' is not specified for use on field 'fizz' of interface 'foo' location.")]
    [InlineData("""
                directive @example on ENUM
                interface foo { fizz(arg: Int @example) : Int }                    
                """,                                                            "Directive 'example' is not specified for use on argument 'arg' of field 'fizz' of interface 'foo' location.")]
    [InlineData("""
                directive @example on INTERFACE
                interface foo @example @example { fizz : Int }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on interface 'foo'.")]
    [InlineData("""
                directive @example on FIELD_DEFINITION
                interface foo  { fizz : Int @example @example }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example @example) : Int }                  
                """,                                                            "Directive 'example' is not repeatable but has been applied multiple times on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on INTERFACE
                interface foo @example { fizz : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on interface 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on FIELD_DEFINITION
                interface foo  { fizz : Int @example }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example) : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on INTERFACE
                interface foo @example { fizz : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                interface foo { fizz : Int @example }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example) : Int }                  
                """,                                                            "Directive 'example' has mandatory argument 'arg1' missing on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example on INTERFACE
                interface foo @example(arg1: 123) { fizz : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on interface 'foo'.")]
    [InlineData("""
                directive @example on FIELD_DEFINITION
                interface foo { fizz : Int  @example(arg1: 123)}                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example(arg1: 123)) : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on INTERFACE
                interface foo @example(arg1: 123) { fizz : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on FIELD_DEFINITION
                interface foo { fizz : Int @example(arg1: 123) }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int  @example(arg1: 123)) : Int }                
                """,                                                            "Directive 'example' does not define argument 'arg1' provided on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on INTERFACE
                interface foo @example(arg1: null) { fizz : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on interface 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on FIELD_DEFINITION
                interface foo { fizz : Int  @example(arg1: null) }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on INTERFACE
                interface foo @example(arg1: null) { fizz : Int }                
                """,                                                             "Directive 'example' has mandatory argument 'arg1' that is specified as null on interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                interface foo { fizz : Int @example(arg1: null) }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """,                                                            "Directive 'example' has mandatory argument 'arg1' that is specified as null on argument 'arg' of field 'fizz' of interface 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }


    [Theory]
    [InlineData("""
                interface foo { fizz(arg1: String! @deprecated): String }            
                """)]
    [InlineData("""
                interface first { first: Int }
                interface foo implements first { bar: Int first: Int }
                """)]
    [InlineData("""
                interface second { second: Int }
                interface first { first: Int }
                interface foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                interface foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                interface foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                interface foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                interface third { third: Int }
                interface second { second: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                interface foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                interface first { bar(args1: Int, args2: String!): Int }
                interface foo implements first { bar(args1: Int, args2: String!): Int }
                """)]
    [InlineData("""
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar(args1: Int, args2: String): Int }
                """)]
    [InlineData("""
                interface first { bar: Int }
                interface foo implements first { bar: Int buzz(args1: Int!): Int }
                """)]
    [InlineData("""
                interface first { bar: Int }
                interface foo implements first { bar: Int! }
                """)]
    [InlineData("""
                interface first { bar: [Int] }
                interface foo implements first { bar: [Int]! }
                """)]
    [InlineData("""
                interface first { bar: [Int] }
                interface foo implements first { bar: [Int!] }
                """)]
    [InlineData("""
                interface first { bar: [Int] }
                interface foo implements first { bar: [Int!]! }
                """)]
    [InlineData("""
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa | bbb
                interface first { bar: ab }
                interface foo implements first { bar: aaa }
                """)]
    [InlineData("""
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa | bbb
                interface first { bar: ab }
                interface foo implements first { bar: bbb }
                """)]
    [InlineData("""
                interface second { second: Int }
                type buzz implements second { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz second: Int }
                """)]
    [InlineData("""
                interface second { second: Int }
                interface buzz implements second { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz second: Int }
                """)]
    public void ImplementsInterface(string schemaText)
    {
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
    }

    [Fact]
    public void ReferenceCreated()
    {
        var schema = new Schema();
        schema.Add("""
                   interface foo { first: Int }
                   type bar implements foo { first: Int }
                   """);
        schema.Validate();

        var foo = schema.Types["foo"];
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
                   directive @d1 on INTERFACE
                   directive @d2 on FIELD_DEFINITION
                   directive @d3(fizz: Int) on ARGUMENT_DEFINITION
                   interface foo @d1
                   {
                       bar(arg: Int @d3(fizz: 4)): [Int] @d2
                   }
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
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
    }
}
