namespace RocketQL.Core.UnitTests.SchemaTests;

public class Schemas
{
    private static readonly string _valid =
        """
            "description"
            schema { 
                query: Fizz
                mutation: Buzz
                subscription: FizzBuzz
            }
        """;
    
    private static readonly string _invalid =
        """
            "description"
            schema { 
                query: Fizz
                query: Buzz
            }
        """;

    [Fact]
    public void AddSchema()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_valid);
        schema.AddSchema(syntaxSchemaNode.Schemas[0]);
        schema.Validate();

        Assert.Equal("description", schema.Definition.Description);
        Assert.Contains(nameof(AddSchema), schema.Definition.Location.Source);
    }

    [Fact]
    public void SchemaAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Merge(_valid);
            schema.Merge(_valid);
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Schema definition is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void OperationTypeAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Merge(_invalid);
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Type already defined for 'QUERY' operation.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

