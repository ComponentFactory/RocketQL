namespace RocketQL.Core.UnitTests.TypeSystemParser;

public class ExtendInputObjectTypeDefinition
{
    [Fact]
    public void Minimum()
    {
        var t = new Core.TypeSystemParser("extend input foo { bar: Integer }");
        var documentNode = t.Parse();

        var input = documentNode.NotNull().ExtendInputObjectTypes.NotNull().One();
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

    [Fact]
    public void Directive()
    {
        var t = new Core.TypeSystemParser("extend input foo @fizz { bar: Integer @buzz }");
        var documentNode = t.Parse();

        var input = documentNode.NotNull().ExtendInputObjectTypes.NotNull().One();
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
    [InlineData("extend input")]
    [InlineData("extend input foo {")]
    [InlineData("extend input foo { bar")]
    [InlineData("extend input foo { bar:")]
    [InlineData("extend input foo { bar: Integer!")]
    [InlineData("extend input foo { bar: Integer! @")]
    [InlineData("extend input foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        var t = new Core.TypeSystemParser(text);
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

    [Fact]
    public void ExtendInputObjectTypeMissingAtLeastOne()
    {
        var t = new Core.TypeSystemParser("extend input foo 42");
        try
        {
            var documentNode = t.Parse();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend input object type must specify at least one of directive or field list.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



