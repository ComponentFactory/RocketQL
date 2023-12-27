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
    // Undefined directive
    [InlineData("type foo @example { fizz : Int }",                         "Undefined directive 'example' defined on object 'foo'.")]
    [InlineData("type foo { fizz : Int @example }",                         "Undefined directive 'example' defined on field 'fizz' of object 'foo'.")]
    [InlineData("type foo { fizz(arg1: Int @example) : Int }",              "Undefined directive 'example' defined on argument 'arg1' of object 'foo'.")]
    // Implements errors
    [InlineData("type foo implements example { fizz : Int }",               "Undefined interface 'example' defined on object 'foo'.")]
    [InlineData("type foo implements Int { fizz : Int }",                   "Cannot implement interface 'Int' defined on object 'foo' because it is a 'scalar'.")]
    [InlineData("""
                scalar example
                type foo implements example { fizz : Int }
                """,                                                        "Cannot implement interface 'example' defined on object 'foo' because it is a 'scalar'.")]
    // Undefined types
    [InlineData("type foo { fizz : Buzz }",                                 "Undefined type 'Buzz' for field 'fizz' of object 'foo'.")]
    [InlineData("type foo { fizz(arg1: Buzz) : String }",                   "Undefined type 'Buzz' for argument 'arg1' of object 'foo'.")]
    // Argument errors
    [InlineData("type foo { fizz(arg1: String, arg1: String): String }",    "Object 'foo' has field 'fizz' with duplicate argument 'arg1'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddObjectType()
    {
        var schema = new Schema();
        schema.Add("type foo { fizz : Int buzz : String } ");
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

