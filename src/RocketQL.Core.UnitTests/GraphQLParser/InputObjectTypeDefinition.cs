namespace RocketQL.Core.UnitTests.GraphQLParser;

public class InputObjectTypeDefinition
{
    [Fact]
    public void Minimum()
    {
        var t = new Core.GraphQLParser("input foo { bar: Integer }");
        var documentNode = t.Parse();

        var input = documentNode.NotNull().InputObjectTypes.NotNull().One();
        Assert.Equal(string.Empty, input.Description);
        Assert.Equal("foo", input.Name);
        input.Directives.NotNull().Count(0);
        var field = input.InputFields.NotNull().One();
        Assert.Equal(string.Empty, field.Description);
        Assert.Equal("bar", field.Name);
        field.DefaultValue.IsNull();
        field.Directives.NotNull().Count(0);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        Assert.False(fieldType.NonNull);
    }

    [Theory]
    [InlineData("\"bar\" input foo { \"fizz\" bar: Integer }")]
    [InlineData("\"\"\"bar\"\"\" input foo { \"\"\"fizz\"\"\" bar: Integer }")]
    public void Description(string schema)
    {
        var t = new Core.GraphQLParser(schema);
        var documentNode = t.Parse();

        var input = documentNode.NotNull().InputObjectTypes.NotNull().One();
        Assert.Equal("bar", input.Description);
        Assert.Equal("foo", input.Name);
        input.Directives.NotNull().Count(0);
        var field = input.InputFields.NotNull().One();
        Assert.Equal("fizz", field.Description);
        Assert.Equal("bar", field.Name);
        field.DefaultValue.IsNull();
        field.Directives.NotNull().Count(0);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        Assert.False(fieldType.NonNull);
    }

    [Fact]
    public void Directive()
    {
        var t = new Core.GraphQLParser("input foo @fizz { bar: Integer @buzz }");
        var documentNode = t.Parse();

        var input = documentNode.NotNull().InputObjectTypes.NotNull().One();
        Assert.Equal(string.Empty, input.Description);
        Assert.Equal("foo", input.Name);
        var directive1 = input.Directives.NotNull().One();
        Assert.Equal("fizz", directive1.Name);
        var field = input.InputFields.NotNull().One();
        Assert.Equal(string.Empty, field.Description);
        Assert.Equal("bar", field.Name);
        var directive2 = field.Directives.NotNull().One();
        Assert.Equal("buzz", directive2.Name);
        TypeNameNode fieldType = (TypeNameNode)field.Type;
        Assert.Equal("Integer", fieldType.Name);
        Assert.False(fieldType.NonNull);
    }

    [Theory]
    [InlineData("input")]
    [InlineData("input foo {")]
    [InlineData("input foo { bar")]
    [InlineData("input foo { bar:")]
    [InlineData("input foo { bar: Integer!")]
    [InlineData("input foo { bar: Integer! @")]
    [InlineData("input foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        var t = new Core.GraphQLParser(text);
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



