namespace RocketQL.Core.UnitTests.SchemaTests;

public class Scalar
{
    private static string _foo =
        """
            "description"
            scalar Foo
        """;

    [Fact]
    public void NameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Merge(_foo);
            schema.Merge(_foo);
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Scalar 'Foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

