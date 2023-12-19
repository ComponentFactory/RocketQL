namespace RocketQL.Core.UnitTests.SchemaTests;

public class Schemas
{
    [Fact]
    public void EmptySchemaMissingQueryOperation()
    {
        var schema = new Schema();
        ValidationException exception = Assert.Throws<ValidationException>(() => schema.Validate());
        Assert.Equal("Schema definition missing mandatory Query operation.", exception.Message);
    }

    [Fact]
    public void SchemaDefinitionCannotBeEmpty()
    {
        var schema = new Schema();
        schema.Add(new SyntaxSchemaDefinitionNode(string.Empty, [], [], new Location()));
        ValidationException exception = Assert.Throws<ValidationException>(() => schema.Validate());
        Assert.Equal("Schema definition does not define any operations.", exception.Message);
    }
}

