namespace RocketQL.Core.UnitTests.Parser;

public class ObjectTypeDefinition
{
    [Fact]
    public void Minimum()
    {
        var t = new Core.Parser("type foo");
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.FieldDefinitions.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" type foo")]
    [InlineData("\"\"\"bar\"\"\" type foo")]
    public void Description(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal("bar", type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.FieldDefinitions.NotNull().Count(0);
    }

    [Theory]
    [InlineData("type foo implements bar", new string[] { "bar" })]
    [InlineData("type foo implements & bar", new string[] { "bar" })]
    [InlineData("type foo implements bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("type foo implements & bar & fizz", new string[] { "bar", "fizz" })]
    [InlineData("type foo implements bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    [InlineData("type foo implements & bar & fizz & buzz", new string[] { "bar", "fizz", "buzz" })]
    public void ImplementsInterface(string schema, string[] implements)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(implements.Length);
        for (int i = 0; i < implements.Length; i++)
            Assert.Equal(implements[i], type.ImplementsInterfaces[i]);
        type.Directives.NotNull().Count(0);
        type.FieldDefinitions.NotNull().Count(0);
    }

    [Fact]
    public void SingleFieldPlain()
    {
        var t = new Core.Parser("type foo { bar: Integer }");
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.FieldDefinitions.NotNull().One();
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        Assert.Equal("bar", field.Name);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
    }

    [Theory]
    [InlineData("type foo { \"fizz\" bar: Integer }")]
    [InlineData("type foo { \"fizz\"bar: Integer }")]
    [InlineData("type foo {\"fizz\"bar: Integer }")]
    [InlineData("type foo {\"\"\"fizz\"\"\"bar: Integer }")]
    public void SingleFieldWithDescription(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.FieldDefinitions.NotNull().One();
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
        var t = new Core.Parser("type foo { bar: Integer @fizz }");
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.FieldDefinitions.NotNull().One();
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
        var t = new Core.Parser("type foo { bar(hello: Integer = 3): Integer }");
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        var field = type.FieldDefinitions.NotNull().One();
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
        var t = new Core.Parser("type foo { bar: Integer fizz: String }");
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.Directives.NotNull().Count(0);
        type.FieldDefinitions.NotNull().Count(2);
        var field1 = type.FieldDefinitions[0];
        field1.Arguments.NotNull().Count(0);
        field1.Directives.NotNull().Count(0);
        Assert.Equal("bar", field1.Name);
        TypeNameNode field1Type = (TypeNameNode)field1.Type;
        Assert.Equal("Integer", field1Type.Name);
        var field2 = type.FieldDefinitions[1];
        field2.Arguments.NotNull().Count(0);
        field2.Directives.NotNull().Count(0);
        Assert.Equal("fizz", field2.Name);
        TypeNameNode field2Type = (TypeNameNode)field2.Type;
        Assert.Equal("String", field2Type.Name);
    }

    [Fact]
    public void Directive()
    {
        var t = new Core.Parser("type foo @bar");
        var documentNode = t.Parse();

        var type = documentNode.NotNull().ObjectTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, type.Description);
        Assert.Equal("foo", type.Name);
        var directive = type.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        type.ImplementsInterfaces.NotNull().Count(0);
        type.FieldDefinitions.NotNull().Count(0);
    }

    [Theory]
    [InlineData("type")]
    [InlineData("type foo {")]
    [InlineData("type foo { bar")]
    [InlineData("type foo { bar:")]
    [InlineData("type foo { bar: Integer")]
    [InlineData("type foo implements")]
    [InlineData("type foo implements bar &")]
    [InlineData("type foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        var t = new Core.Parser(text);
        try
        {
            var documentNode = t.Parse();
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



