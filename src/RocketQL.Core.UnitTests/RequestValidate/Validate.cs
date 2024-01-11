using RocketQL.Core.Nodes;

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

    [Fact]
    public void SchemaNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxSchemaDefinitionNode("", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Schema definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void DirectiveNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxDirectiveDefinitionNode("", "Example", [], false, DirectiveLocations.ENUM, new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Directive definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ScalarNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxScalarTypeDefinitionNode("", "Example", [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Scalar definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ObjectNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxObjectTypeDefinitionNode("", "Example", [], [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Object definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void InterfaceNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxInterfaceTypeDefinitionNode("", "Example", [], [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Interface definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void UnionNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxUnionTypeDefinitionNode("", "Example", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Union definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void EnumNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxEnumTypeDefinitionNode("", "Example", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Enum definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void InputObjectNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxInputObjectTypeDefinitionNode("", "Example", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Input object definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendSchemaNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendSchemaDefinitionNode([], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend schema definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendScalarNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendScalarTypeDefinitionNode("Example", [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend scalar definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendObjectNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendObjectTypeDefinitionNode("Example", [], [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend object definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendInterfaceNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendInterfaceTypeDefinitionNode("Example", [], [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend interface definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendUnionNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendUnionTypeDefinitionNode("Example", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend union definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendEnumNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendEnumTypeDefinitionNode("Example", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend enum definition not allowed in a schema.", exception.Message);
    }

    [Fact]
    public void ExtendInputObjectNotAllowed()
    {
        var schema = new Schema();
        schema.Add("type Query { fizz: Int }");
        schema.Validate();
        var request = new Request();
        request.Add(new SyntaxExtendInputObjectTypeDefinitionNode("Example", [], [], new()));
        var exception = Assert.Throws<ValidationException>(() => request.ValidateSchema(schema));
        Assert.Equal("Extend input object definition not allowed in a schema.", exception.Message);
    }
}
