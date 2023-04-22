using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;

namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ExtendObjectTypeDefinition
{
    [Theory]
    [InlineData("extend type foo implements bar", new string[] { "bar" })]
    [InlineData("extend type foo implements & bar", new string[] { "bar" })]
    [InlineData("extend type foo implements bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("extend type foo implements & bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("extend type foo implements bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    [InlineData("extend type foo implements & bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    public void ImplementsInterface(string schema, string[] implements)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(implements.Length);
        for (int i = 0; i < implements.Length; i++)
            Assert.Equal(implements[i], type.ImplementsInterfaces[i]);
        type.Directives.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Fact]
    public void SingleFieldPlain()
    {
        var documentNode = Document.SchemaDeserialize("extend type foo { bar: Integer }");

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
    }

    [Theory]
    [InlineData("extend type foo { \"fizz\" bar: Integer }")]
    [InlineData("extend type foo { \"fizz\"bar: Integer }")]
    [InlineData("extend type foo {\"fizz\"bar: Integer }")]
    [InlineData("extend type foo {\"\"\"fizz\"\"\"bar: Integer }")]
    public void SingleFieldWithDescription(string schema)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        Assert.Equal("fizz", field.Description);
        Assert.Equal("bar", field.Name);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
    }

    [Fact]
    public void SingleFieldWithDirective()
    {
        var documentNode = Document.SchemaDeserialize("extend type foo { bar: Integer @fizz }");

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Arguments.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        var directive = field.Directives.NotNull().One();
        Assert.Equal("fizz", directive.Name);
        directive.Arguments.NotNull().Count(0);
    }

    [Fact]
    public void SingleFieldWithArgument()
    {
        var documentNode = Document.SchemaDeserialize("extend type foo { bar(hello: Integer = 3): Integer }");

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Directives.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        var argument = field.Arguments.NotNull().One();
        Assert.Equal("hello", argument.Name);
        TypeNameNode argumentType = (TypeNameNode)argument.Type;
        Assert.Equal("Integer", argumentType.Name);
        argument.Directives.NotNull().Count(0);
        argument.DefaultValue.NotNull();
        IntValueNode defaultValue = (IntValueNode)argument.DefaultValue;
        Assert.Equal("3", defaultValue.Value);
    }

    [Fact]
    public void TwoFieldsPlain()
    {
        var documentNode = Document.SchemaDeserialize("extend type foo { bar: Integer fizz: String }");

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.Fields.NotNull().Count(2);
        var field1 = type.Fields[0];
        field1.Arguments.NotNull().Count(0);
        field1.Directives.NotNull().Count(0);
        Assert.Equal("bar", field1.Name);
        TypeNameNode field1Type = (TypeNameNode)field1.Type;
        Assert.Equal("Integer", field1Type.Name);
        var field2 = type.Fields[1];
        field2.Arguments.NotNull().Count(0);
        field2.Directives.NotNull().Count(0);
        Assert.Equal("fizz", field2.Name);
        TypeNameNode field2Type = (TypeNameNode)field2.Type;
        Assert.Equal("String", field2Type.Name);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Document.SchemaDeserialize("extend type foo @bar");

        var type = documentNode.NotNull().ExtendObjectTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        var directive = type.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Theory]
    [InlineData("extend type")]
    [InlineData("extend type foo {")]
    [InlineData("extend type foo { bar")]
    [InlineData("extend type foo { bar:")]
    [InlineData("extend type foo { bar: Integer")]
    [InlineData("extend type foo implements")]
    [InlineData("extend type foo implements bar &")]
    [InlineData("extend type foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        try
        {
            var documentNode = Document.SchemaDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Unexpected end of file encountered.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void ExtendObjectTypeMissingAtLeastOne()
    {
        try
        {
            var documentNode = Document.SchemaDeserialize("extend type foo 42");
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend type must specify at least one of interface, directive or field set.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



