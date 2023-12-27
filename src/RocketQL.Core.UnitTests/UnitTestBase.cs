namespace RocketQL.Core.UnitTests;

public class UnitTestBase
{
    public void SchemaValidationException(string schemaTest, string message)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            Assert.Equal(message, ex.Message);
        }
    }

    public void SchemaValidationException(string schemaTest1, string schemaTest2, string message)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest1);
            schema.Add(schemaTest2);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            Assert.Equal(message, ex.Message);
        }
    }
}

