namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Input : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        var schemaText = "input foo { fizz: Int }";
        SchemaValidationException(schemaText, schemaText, "Input object 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("input __foo { fizz : Int }",                               "Input object '__foo' not allowed to start with two underscores.")]
    [InlineData("input foo { __fizz : Int }",                               "Input object 'foo' has field '__fizz' not allowed to start with two underscores.")]
    // Undefined directive
    [InlineData("input foo @example { fizz : Int }",                        "Undefined directive 'example' defined on input object 'foo'.")]
    [InlineData("input foo { fizz : Int @example }",                        "Undefined directive 'example' defined on input field 'fizz' of input object 'foo'.")]
    // Undefined types
    [InlineData("input foo { fizz : Buzz }",                                "Undefined type 'Buzz' for input field 'fizz' of input object 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddInputObjectType()
    {
        var schema = new Schema();
        schema.Add("input foo { fizz: Int }");
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.NotNull(foo.InputFields["fizz"]);
        Assert.Contains(nameof(AddInputObjectType), foo.Location.Source);
    }
}

