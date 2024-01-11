namespace RocketQL.Core.UnitTests.RequestValidation;

public class Validate : UnitTestBase
{
    [Fact]
    public void AttachedSchemaNotValidated()
    {
        var schema = new Schema();
        var request = new Request();
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Provided schema has not been validated.", exception.Message);
    }

    [Fact]
    public void ValidateSchemaNotPerformed()
    {
        var schema = new Schema();
        var request = new Request();
        var exception = Assert.Throws<ValidationException>(() => request.ValidateVariables(BooleanValueNode.True));
        Assert.Equal("Provided schema has not been validated.", exception.Message);
    }
}

