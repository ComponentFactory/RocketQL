using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Scalar : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationException("scalar foo", "scalar foo", "Scalar 'foo' is already defined.");
    }

    [Theory]
    [InlineData("Int")]
    [InlineData("Float")]
    [InlineData("String")]
    [InlineData("Boolean")]
    [InlineData("ID")]
    public void CannotUsePredefinedName(string scalar)
    {
        SchemaValidationException($"scalar {scalar}", $"Scalar '{scalar}' is already defined.");
    }

    [Theory]
    [InlineData("scalar __foo",             "Scalar '__foo' not allowed to start with two underscores.")]
    [InlineData("scalar foo @example",      "Undefined directive 'example' defined on scalar 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddScalarType()
    {
        var schema = new Schema();
        schema.Add("scalar foo");
        schema.Validate();

        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddScalarType), foo.Location.Source);
    }
}

