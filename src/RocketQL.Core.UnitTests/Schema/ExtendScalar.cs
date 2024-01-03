namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendScalar : UnitTestBase
{
    [Theory]
    [InlineData("extend scalar foo",                    "Scalar 'foo' cannot be extended because it is not defined.")]
    [InlineData("""
                extend scalar foo 
                scalar foo
                """,                                    "Scalar 'foo' cannot be extended because it is not defined.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }
}

