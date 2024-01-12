﻿namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class InterfaceTypeDefinition : UnitTestBase
{
    [Fact]
    public void Minimum()
    {
        var documentNode = Serialization.SchemaDeserialize("interface foo");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" interface foo")]
    [InlineData("\"\"\"bar\"\"\" interface foo")]
    public void Description(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("bar", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Theory]
    [InlineData("interface foo implements bar", new string[] { "bar" })]
    [InlineData("interface foo implements & bar", new string[] { "bar" })]
    [InlineData("interface foo implements bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("interface foo implements & bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("interface foo implements bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    [InlineData("interface foo implements & bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    public void ImplementsInterface(string schema, string[] implements)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(implements.Length);
        for (int i = 0; i < implements.Length; i++)
            Assert.Equal(implements[i], type.ImplementsInterfaces[i].Name);
        type.Directives.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Fact]
    public void SingleFieldPlain()
    {
        var documentNode = Serialization.SchemaDeserialize("interface foo { bar: Integer }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        SyntaxTypeNameNode fieldType = (SyntaxTypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
    }

    [Theory]
    [InlineData("interface foo { \"fizz\" bar: Integer }")]
    [InlineData("interface foo { \"fizz\"bar: Integer }")]
    [InlineData("interface foo {\"fizz\"bar: Integer }")]
    [InlineData("interface foo {\"\"\"fizz\"\"\"bar: Integer }")]
    public void SingleFieldWithDescription(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        Assert.Equal("fizz", field.Description);
        Assert.Equal("bar", field.Name);
        SyntaxTypeNameNode fieldType = (SyntaxTypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
    }

    [Fact]
    public void SingleFieldWithDirective()
    {
        var documentNode = Serialization.SchemaDeserialize("interface foo { bar: Integer @fizz }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Arguments.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        SyntaxTypeNameNode fieldType = (SyntaxTypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        var directive = field.Directives.NotNull().One();
        Assert.Equal("fizz", directive.Name);
        directive.Arguments.NotNull().Count(0);
    }

    [Fact]
    public void SingleFieldWithArgument()
    {
        var documentNode = Serialization.SchemaDeserialize("interface foo { bar(hello: Integer = 3): Integer }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.Fields.NotNull().One();
        field.Directives.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        SyntaxTypeNameNode fieldType = (SyntaxTypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        var argument = field.Arguments.NotNull().One();
        Assert.Equal("hello", argument.Name);
        SyntaxTypeNameNode argumentType = (SyntaxTypeNameNode)argument.Type;
        Assert.Equal("Integer", argumentType.Name);
        argument.Directives.NotNull().Count(0);
        argument.DefaultValue.NotNull();
        IntValueNode defaultValue = (IntValueNode)argument.DefaultValue;
        Assert.Equal("3", defaultValue.Value);
    }

    [Fact]
    public void TwoFieldsPlain()
    {
        var documentNode = Serialization.SchemaDeserialize("interface foo { bar: Integer fizz: String }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.Fields.NotNull().Count(2);
        var field1 = type.Fields[0];
        field1.Arguments.NotNull().Count(0);
        field1.Directives.NotNull().Count(0);
        Assert.Equal("bar", field1.Name);
        SyntaxTypeNameNode field1Type = (SyntaxTypeNameNode)field1.Type;
        Assert.Equal("Integer", field1Type.Name);
        var field2 = type.Fields[1];
        field2.Arguments.NotNull().Count(0);
        field2.Directives.NotNull().Count(0);
        Assert.Equal("fizz", field2.Name);
        SyntaxTypeNameNode field2Type = (SyntaxTypeNameNode)field2.Type;
        Assert.Equal("String", field2Type.Name);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("interface foo @bar");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxInterfaceTypeDefinitionNode>(definition);
        var type = ((SyntaxInterfaceTypeDefinitionNode)definition);
        Assert.Equal("", type.Description);
        Assert.Equal("foo", type.Name);
        var directive = type.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Theory]
    [InlineData("interface")]
    [InlineData("interface foo {")]
    [InlineData("interface foo { bar")]
    [InlineData("interface foo { bar:")]
    [InlineData("interface foo { bar: Integer")]
    [InlineData("interface foo implements")]
    [InlineData("interface foo implements bar &")]
    [InlineData("interface foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize(text);
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
}



