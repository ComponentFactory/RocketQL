namespace RocketQL.Core.UnitTests.SchemaValidation;

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
    // Undefined directive
    [InlineData("interface foo @example { fizz : Int }",                        "Undefined directive 'example' defined on interface 'foo'.")]
    [InlineData("interface foo { fizz : Int @example }",                        "Undefined directive 'example' defined on field 'fizz' of interface 'foo'.")]
    [InlineData("interface foo { fizz(arg1: Int @example) : Int }",             "Undefined directive 'example' defined on argument 'arg1' of interface 'foo'.")]
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
    public void AddInterfaceType()
    {
        var schema = new Schema();
        schema.Add("interface foo { fizz : Int }");
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.Fields);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.Contains(nameof(AddInterfaceType), foo.Location.Source);
    }
}
