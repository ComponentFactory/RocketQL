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
    [InlineData("directive @__foo on ENUM",                                 "Directive '__foo' not allowed to start with two underscores.")]
    [InlineData("directive @foo(__arg1: String) on ENUM",                   "Directive 'foo' has argument '__arg1' not allowed to start with two underscores.")]
    // Undefined directive
    [InlineData("directive @foo(arg1: String @example) on ENUM",            "Undefined directive 'example' defined on argument 'arg1' of directive 'foo'.")]
    // Undefined types
    [InlineData("directive @foo(arg1: Fizz) on ENUM",                       "Undefined type 'Fizz' for argument 'arg1' of directive 'foo'.")]
    // Argument errors
    [InlineData("directive @foo(arg1: String, arg1: String) on ENUM",     "Directive 'foo' has duplicate argument 'arg1'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddDirective()
    {
        var schema = new Schema();
        schema.Add("directive @foo on ENUM");
        schema.Validate();

        Assert.Single(schema.Directives);
        var foo = schema.Directives["foo"];
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddDirective), foo.Location.Source);
    }
}

