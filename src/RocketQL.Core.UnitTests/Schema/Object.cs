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
    // Double underscores
    [InlineData("type __foo { fizz : Int }",                                "Object '__foo' not allowed to start with two underscores.")]
    [InlineData("type foo { __fizz : Int }",                                "Object 'foo' has field '__fizz' not allowed to start with two underscores.")]
    [InlineData("type foo { fizz(__arg1: String): String }",                "Object 'foo' has field 'fizz' with argument '__arg1' not allowed to start with two underscores.")]
    //// Undefined directive
    [InlineData("type foo @example { fizz : Int }",                         "Undefined directive 'example' defined on object 'foo'.")]
    [InlineData("type foo { fizz : Int @example }",                         "Undefined directive 'example' defined on field 'fizz' of object 'foo'.")]
    [InlineData("type foo { fizz(arg1: Int @example) : Int }",              "Undefined directive 'example' defined on argument 'arg1' of object 'foo'.")]
    //// Implements errors
    [InlineData("type foo implements example { fizz : Int }",               "Undefined interface 'example' defined on object 'foo'.")]
    [InlineData("type foo implements Int { fizz : Int }",                   "Cannot implement interface 'Int' defined on object 'foo' because it is a 'scalar'.")]
    [InlineData("""
                interface second  { second: Int }
                interface first implements second { first: Int }
                type foo implements first { bar: Int }
                """,                                                        "Object 'foo' is missing implements 'second' because it is declared on interface 'first'.")]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int }
                interface first implements second { first: Int }
                type foo implements first & second { bar: Int }
                """,                                                        "Object 'foo' is missing implements 'third' because it is declared on interface 'second'.")]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int }
                interface first implements third { first: Int }
                type foo implements first & second { bar: Int }
                """,                                                        "Object 'foo' is missing implements 'third' because it is declared on interface 'first'.")]
    [InlineData("""
                scalar example
                type foo implements example { fizz : Int }
                """,                                                        "Cannot implement interface 'example' defined on object 'foo' because it is a 'scalar'.")]
    // Undefined types
    [InlineData("type foo { fizz : Buzz }",                                 "Undefined type 'Buzz' for field 'fizz' of object 'foo'.")]
    [InlineData("type foo { fizz(arg1: Buzz) : String }",                   "Undefined type 'Buzz' for argument 'arg1' of object 'foo'.")]
    // Argument errors
    [InlineData("type foo { fizz(arg1: String, arg1: String): String }",    "Object 'foo' has field 'fizz' with duplicate argument 'arg1'.")]
    [InlineData("type foo { fizz(arg1: String! @deprecated): String }",     "Cannot use @deprecated directive on non-null argument 'arg1' of field 'fizz' of object 'foo'.")]
    [InlineData("""                
                input foo { fizz : Int }
                type bar { buzz(arg1: foo): String }
                """,                                                        "Object 'bar' has field 'buzz' with argument 'arg1' of type 'foo' that is not an input type.")]
    // Field errors
    [InlineData("type foo { fizz: String fizz:String }",                    "Object 'foo' has duplicate field 'fizz'.")]
    [InlineData("""
                input foo { fizz : Int }
                type bar { buzz: foo }                 
                """,                                                        "Object 'bar' has field 'buzz' with type 'foo' that is not an output type.")]
    [InlineData("""
                interface first { first: Int }
                type foo implements first { bar: Int }
                """,                                                        "Object 'foo' is missing field 'first' declared on interface 'first'.")]
    [InlineData("""
                interface second { second: Int }
                interface first implements second { first: Int }
                type foo implements first & second { bar: Int first: Int }
                """,                                                        "Object 'foo' is missing field 'second' declared on interface 'second'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Theory]
    [InlineData("""
                interface first { first: Int }
                type foo implements first { bar: Int first: Int }
                """)]
    [InlineData("""
                interface second { second: Int }
                interface first { first: Int }
                type foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                interface second { second: Int }
                interface first implements second { first: Int }
                type foo implements first & second { bar: Int first: Int second: Int }
                """)]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int }
                interface first implements second { first: Int }
                type foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                interface third { third: Int }
                interface second implements third { second: Int }
                interface first implements third { first: Int }
                type foo implements first & second & third { bar: Int first: Int second: Int third: Int }
                """)]
    [InlineData("""
                interface third { third: Int }
                interface second { second: Int }
                interface first implements second & third { first: Int }
                type foo implements first & second & third { bar: Int first: Int second: Int third: Int }
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
    public void AddObjectType()
    {
        var schema = new Schema();
        schema.Add("type foo { fizz : Int buzz : String }");
        schema.Validate();

        var foo = schema.Types["foo"] as ObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.Fields.Count);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.NotNull(foo.Fields["buzz"]);
        Assert.Contains(nameof(AddObjectType), foo.Location.Source);
    }
}

