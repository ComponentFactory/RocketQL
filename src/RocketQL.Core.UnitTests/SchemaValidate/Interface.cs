namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Interface : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationSinglePathException("""
                                            type Query { alpha: Int} 
                                            interface foo { fizz : Int }
                                            """,
                                            "interface foo { fizz : Int }",
                                            "Interface 'foo' is already defined.",
                                            "interface foo");
    }

    [Theory]
    // At least one field
    [InlineData("""
                type Query { alpha: Int }
                interface foo
                """, "Interface 'foo' must have at least one field.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo {}
                """, "Interface 'foo' must have at least one field.")]
    // Double underscores
    [InlineData("""
                type Query { alpha: Int }
                interface __foo { fizz : Int }
                """, "Interface '__foo' not allowed to start with two underscores.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { __fizz : Int }
                """, "Interface 'foo' has field '__fizz' not allowed to start with two underscores.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz(__arg1: String): String }
                """, "Interface 'foo' has field 'fizz' with argument '__arg1' not allowed to start with two underscores.")]
    // Implements errors
    [InlineData("""
                type Query { alpha: Int }
                interface foo implements example { fizz : Int }
                """, "Undefined interface 'example' defined on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo implements Int { fizz : Int }
                """, "Cannot implement interface 'Int' defined on interface 'foo' because it is a 'scalar'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first  { first: Int }
                interface foo implements first & first { first : Int }
                """, "Interface 'foo' has duplicate interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface second  { second: Int }
                interface first implements second { first: Int second: Int }
                interface foo implements first { bar: Int first: Int second: Int }
                """, "Interface 'foo' is missing implements 'second' because it is declared on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                scalar example
                interface foo implements example { fizz : Int }
                """, "Cannot implement interface 'example' defined on interface 'foo' because it is a 'scalar'.")]
    // Undefined types
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz : Buzz }
                """, "Undefined type 'Buzz' for field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz(arg1: Buzz) : String }
                """, "Undefined type 'Buzz' for argument 'arg1' of interface 'foo'.")]
    // Argument errors
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz(arg1: String, arg1: String): String }
                """, "Interface 'foo' has field 'fizz' with duplicate argument 'arg1'.")]
    [InlineData("""                
                type Query { alpha: Int }
                interface foo { fizz : Int }
                interface bar { buzz(arg1: foo): String }
                """, "Interface 'bar' has field 'buzz' with argument 'arg1' of type 'foo' that is not an input type.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar: Int }
                """, "Interface 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int, args2: Int): Int }
                interface foo implements first { bar(args2: Int): Int }
                """, "Interface 'foo' field 'bar' is missing argument 'args1' declared on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar(args1: String): Int }
                """, "Interface 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar(args1: Int!): Int }
                """, "Interface 'foo' field 'bar' argument 'args1' has different type to the declared interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: Int }
                interface foo implements first { bar(args1: Int!): Int }
                """, "Interface 'foo' field 'bar' argument 'args1' cannot be non-null type because not declared on interface 'first'.")]
    // Field errors
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz: String fizz:String }
                """, "Interface 'foo' has duplicate field 'fizz'.")]
    [InlineData("""
                type Query { alpha: Int }
                input foo { fizz : Int }
                interface bar { buzz: foo }                 
                """, "Interface 'bar' has field 'buzz' with type 'foo' that is not an output type.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { first: Int }
                interface foo implements first { bar: Int }
                """, "Interface 'foo' is missing field 'first' declared on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: Int! }
                interface foo implements first { bar: Int }
                """, "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: Int }
                interface foo implements first { bar: [Int] }
                """, "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface first { bar: [Int] }
                interface foo implements first { bar: Int }
                """, "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa
                interface first { bar: ab }
                interface foo implements first { bar: bbb }
                """, "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface third { second: Int }
                type buzz implements third { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz }
                """, "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface third { second: Int }
                interface buzz implements third { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz }
                """, "Interface 'foo' field 'bar' return type not a sub-type of matching field on interface 'first'.")]
    // Directive errors
    [InlineData("""
                type Query { alpha: Int }
                interface foo @example { fizz : Int }
                """, "Undefined directive '@example' defined on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz : Int @example }
                """, "Undefined directive '@example' defined on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { fizz(arg1: Int @example) : Int }
                """, "Undefined directive '@example' defined on argument 'arg1' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                interface foo @example { fizz : Int }                    
                """, "Directive '@example' is not specified for use on interface 'foo' location.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                interface foo { fizz : Int @example }                    
                """, "Directive '@example' is not specified for use on field 'fizz' of interface 'foo' location.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ENUM
                interface foo { fizz(arg: Int @example) : Int }                    
                """, "Directive '@example' is not specified for use on argument 'arg' of field 'fizz' of interface 'foo' location.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on INTERFACE
                interface foo @example @example { fizz : Int }                  
                """, "Directive '@example' is not repeatable but has been applied multiple times on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on FIELD_DEFINITION
                interface foo  { fizz : Int @example @example }                  
                """, "Directive '@example' is not repeatable but has been applied multiple times on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example @example) : Int }                  
                """, "Directive '@example' is not repeatable but has been applied multiple times on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on INTERFACE
                interface foo @example { fizz : Int }                  
                """, "Directive '@example' has mandatory argument 'arg1' missing on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on FIELD_DEFINITION
                interface foo  { fizz : Int @example }                  
                """, "Directive '@example' has mandatory argument 'arg1' missing on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example) : Int }                  
                """, "Directive '@example' has mandatory argument 'arg1' missing on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on INTERFACE
                interface foo @example { fizz : Int }                  
                """, "Directive '@example' has mandatory argument 'arg1' missing on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                interface foo { fizz : Int @example }                  
                """, "Directive '@example' has mandatory argument 'arg1' missing on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example) : Int }                  
                """, "Directive '@example' has mandatory argument 'arg1' missing on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on INTERFACE
                interface foo @example(arg1: 123) { fizz : Int }                
                """, "Directive '@example' does not define argument 'arg1' provided on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on FIELD_DEFINITION
                interface foo { fizz : Int  @example(arg1: 123)}                
                """, "Directive '@example' does not define argument 'arg1' provided on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example(arg1: 123)) : Int }                
                """, "Directive '@example' does not define argument 'arg1' provided on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on INTERFACE
                interface foo @example(arg1: 123) { fizz : Int }                
                """, "Directive '@example' does not define argument 'arg1' provided on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on FIELD_DEFINITION
                interface foo { fizz : Int @example(arg1: 123) }                
                """, "Directive '@example' does not define argument 'arg1' provided on field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int  @example(arg1: 123)) : Int }                
                """, "Directive '@example' does not define argument 'arg1' provided on argument 'arg' of field 'fizz' of interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on INTERFACE
                interface foo @example(arg1: null) { fizz : Int }                
                """, "Argument 'arg1' of directive '@example' of interface 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on FIELD_DEFINITION
                interface foo { fizz : Int  @example(arg1: null) }                
                """, "Argument 'arg1' of directive '@example' of interface 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """, "Argument 'arg1' of directive '@example' of interface 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on INTERFACE
                interface foo @example(arg1: null) { fizz : Int }                
                """, "Argument 'arg1' of directive '@example' of interface 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on FIELD_DEFINITION
                interface foo { fizz : Int @example(arg1: null) }                
                """, "Argument 'arg1' of directive '@example' of interface 'foo' has a default value incompatible with the type.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @example(arg0: Int arg1: Int!) on ARGUMENT_DEFINITION
                interface foo { fizz(arg: Int @example(arg1: null)) : Int }                
                """, "Argument 'arg1' of directive '@example' of interface 'foo' has a default value incompatible with the type.")]
    public void ValidationSingleExceptions(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
    }

    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                interface foo implements first & second { bar: Int first: Int }
                """, "Interface 'foo' is missing field 'second' declared on interface 'first'.",
                                                                                "Interface 'foo' is missing field 'second' declared on interface 'second'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                interface foo implements first & second { bar: Int first: Int second: Int third: Int }
                """, "Interface 'foo' is missing implements 'third' because it is declared on interface 'first'.",
                                                                                "Interface 'foo' is missing implements 'third' because it is declared on interface 'second'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                interface foo implements first & second { bar: Int first: Int second: Int third: Int}
                """, "Interface 'foo' is missing implements 'third' because it is declared on interface 'first'.",
                                                                                "Interface 'foo' is missing implements 'third' because it is declared on interface 'second'.")]
    public void ValidationMultipleExceptions(string schemaText, params string[] messages)
    {
        SchemaValidationMultipleExceptions(schemaText, messages);
    }


    [Theory]
    [InlineData("""
                type Query { query: Int }
                interface foo { fizz(arg1: String! @deprecated): String }            
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { first: Int }
                interface foo implements first { bar: Int first: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                interface first { first: Int }
                interface foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                interface first implements second { first: Int second: Int }
                interface foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                interface foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface third { third: Int }
                interface second implements third { second: Int third: Int }
                interface first implements third { first: Int third: Int }
                interface foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface third { third: Int }
                interface second { second: Int }
                interface first implements second & third { first: Int second: Int third: Int }
                interface foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar(args1: Int, args2: String!): Int }
                interface foo implements first { bar(args1: Int, args2: String!): Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar(args1: Int): Int }
                interface foo implements first { bar(args1: Int, args2: String): Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: Int }
                interface foo implements first { bar: Int buzz(args1: Int!): Int }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: Int }
                interface foo implements first { bar: Int! }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: [Int] }
                interface foo implements first { bar: [Int]! }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: [Int] }
                interface foo implements first { bar: [Int!] }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface first { bar: [Int] }
                interface foo implements first { bar: [Int!]! }
                """)]
    [InlineData("""
                type Query { query: Int }
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa | bbb
                interface first { bar: ab }
                interface foo implements first { bar: aaa }
                """)]
    [InlineData("""
                type Query { query: Int }
                type aaa { aaa: Int }
                type bbb { bbb: Int }
                union ab = aaa | bbb
                interface first { bar: ab }
                interface foo implements first { bar: bbb }
                """)]
    [InlineData("""
                type Query { query: Int }
                interface second { second: Int }
                type buzz implements second { second: Int }
                interface first { bar: second }
                interface foo implements first { bar: buzz second: Int }
                """)]
    [InlineData("""
                type Query { query: Int }
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
                   type Query { query: Int }
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
                   type Query { query: Int }
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
    }

    [Fact]
    public void ValidTypeCheckOnDefaultValue()
    {
        SchemaValidationNoException("""
                                    type Query { query: Int }
                                    interface foo 
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
                                        interface foo 
                                        {
                                           a(arg: Int = 3.13): Int
                                        }
                                        """,
                                        "Argument 'arg' of field 'a' of interface 'foo' has a default value incompatible with the type.");
    }
}
