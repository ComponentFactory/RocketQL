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
    [InlineData("interface __foo { fizz : Int }",                                   "Interface '__foo' not allowed to start with two underscores.")]
    [InlineData("interface foo { __fizz : Int }",                                   "Interface 'foo' has field '__fizz' not allowed to start with two underscores.")]
    [InlineData("interface foo { fizz(__arg1: String): String }",                   "Interface 'foo' has field 'fizz' with argument '__arg1' not allowed to start with two underscores.")]
    // Undefined directive
    [InlineData("interface foo @example { fizz : Int }",                            "Undefined directive 'example' defined on interface 'foo'.")]
    [InlineData("interface foo { fizz : Int @example }",                            "Undefined directive 'example' defined on field 'fizz' of interface 'foo'.")]
    [InlineData("interface foo { fizz(arg1: Int @example) : Int }",                 "Undefined directive 'example' defined on argument 'arg1' of interface 'foo'.")]
    // Implements errors
    [InlineData("interface foo implements example { fizz : Int }",                  "Undefined interface 'example' defined on interface 'foo'.")]
    [InlineData("interface foo implements Int { fizz : Int }",                      "Cannot implement interface 'Int' defined on interface 'foo' because it is a 'scalar'.")]
    [InlineData("""
                scalar example
                interface foo implements example { fizz : Int }
                """,                                                                "Cannot implement interface 'example' defined on interface 'foo' because it is a 'scalar'.")]
    // Undefined types
    [InlineData("interface foo { fizz : Buzz }",                                    "Undefined type 'Buzz' for field 'fizz' of interface 'foo'.")]
    [InlineData("interface foo { fizz(arg1: Buzz) : String }",                      "Undefined type 'Buzz' for argument 'arg1' of interface 'foo'.")]
    // Argument errors
    [InlineData("interface foo { fizz(arg1: String, arg1: String): String }",       "Interface 'foo' has field 'fizz' with duplicate argument 'arg1'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
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
