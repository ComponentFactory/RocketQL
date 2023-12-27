namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ExtendInterfaceTypeDefinition : UnitTestBase
{
    [Theory]
    [InlineData("extend interface foo implements bar", new string[] { "bar" })]
    [InlineData("extend interface foo implements & bar", new string[] { "bar" })]
    [InlineData("extend interface foo implements bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("extend interface foo implements & bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("extend interface foo implements bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    [InlineData("extend interface foo implements & bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    public void ImplementsInterface(string schema, string[] implements)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
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
        var documentNode = Serialization.SchemaDeserialize("extend interface foo { bar: Integer }");

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
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
    [InlineData("extend interface foo { \"fizz\" bar: Integer }")]
    [InlineData("extend interface foo { \"fizz\"bar: Integer }")]
    [InlineData("extend interface foo {\"fizz\"bar: Integer }")]
    [InlineData("extend interface foo {\"\"\"fizz\"\"\"bar: Integer }")]
    public void SingleFieldWithDescription(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
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
        var documentNode = Serialization.SchemaDeserialize("extend interface foo { bar: Integer @fizz }");

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
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
        var documentNode = Serialization.SchemaDeserialize("extend interface foo { bar(hello: Integer = 3): Integer }");

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
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
        var documentNode = Serialization.SchemaDeserialize("extend interface foo { bar: Integer fizz: String }");

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
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
        var documentNode = Serialization.SchemaDeserialize("extend interface foo @bar");

        var type = documentNode.NotNull().ExtendInterfaceTypes.NotNull().One();
        Assert.Equal("foo", type.Name);
        var directive = type.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Fields.NotNull().Count(0);
    }

    [Theory]
    [InlineData("extend interface")]
    [InlineData("extend interface foo {")]
    [InlineData("extend interface foo { bar")]
    [InlineData("extend interface foo { bar:")]
    [InlineData("extend interface foo { bar: Integer")]
    [InlineData("extend interface foo implements")]
    [InlineData("extend interface foo implements bar &")]
    [InlineData("extend interface foo @")]
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

    [Fact]
    public void ExtendInterfaceTypeMissingAtLeastOne()
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize("extend interface foo 42");
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend interface must specify at least one of interface, directive or field set.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



