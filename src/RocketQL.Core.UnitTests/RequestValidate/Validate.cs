using RocketQL.Core.Nodes;

namespace RocketQL.Core.UnitTests.RequestValidation;

public class Validate : UnitTestBase
{
    [Fact]
    public void SchemaNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxSchemaDefinitionNode("", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Schema definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void DirectiveNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxDirectiveDefinitionNode("", "Example", [], false, DirectiveLocations.ENUM, Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Directive definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ScalarNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxScalarTypeDefinitionNode("", "Example", [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Scalar definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ObjectNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxObjectTypeDefinitionNode("", "Example", [], [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Type definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void InterfaceNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxInterfaceTypeDefinitionNode("", "Example", [], [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Interface definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void UnionNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxUnionTypeDefinitionNode("", "Example", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Union definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void EnumNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxEnumTypeDefinitionNode("", "Example", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Enum definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void InputObjectNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxInputObjectTypeDefinitionNode("", "Example", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Input object definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendSchemaNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendSchemaDefinitionNode([], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend schema definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendScalarNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendScalarTypeDefinitionNode("Example", [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend scalar definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendObjectNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendObjectTypeDefinitionNode("Example", [], [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend object definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendInterfaceNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendInterfaceTypeDefinitionNode("Example", [], [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend interface definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendUnionNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendUnionTypeDefinitionNode("Example", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend union definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendEnumNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendEnumTypeDefinitionNode("Example", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend enum definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }

    [Fact]
    public void ExtendInputObjectNotAllowed()
    {
        var schema = new SchemaBuilder();
        schema.AddFromString("type Query { fizz: Int }");
        var request = new RequestBuilder();
        request.AddSyntaxNode(new SyntaxExtendInputObjectTypeDefinitionNode("Example", [], [], Location.Empty));
        var exception = Assert.Throws<ValidationException>(() => request.Build(schema.Build()));
        Assert.Equal("Extend input object definition not allowed in a schema.", exception.Message);
        Assert.Equal("", exception.CommaPath);
    }
}
