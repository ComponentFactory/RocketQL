namespace RocketQL.Core.UnitTests.Parser;

public class EnumTypeDefinition
{
    [Fact]
    public void OneEnum()
    {
        var t = new Core.Parser("enum foo { BUZZ }");
        var documentNode = t.Parse();

        var enums = documentNode.NotNull().EnumTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, enums.Description);
        Assert.Equal("foo", enums.Name);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal(string.Empty, enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
        enumValue.Directives.NotNull().Count(0);
    }

    [Fact]
    public void TwoEnums()
    {
        var t = new Core.Parser("enum foo { FIZZ BUZZ }");
        var documentNode = t.Parse();

        var enums = documentNode.NotNull().EnumTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, enums.Description);
        Assert.Equal("foo", enums.Name);
        enums.EnumValues.NotNull().Count(2);
        var enumValue1 = enums.EnumValues[0];
        Assert.Equal(string.Empty, enumValue1.Description);
        Assert.Equal("FIZZ", enumValue1.Name);
        enumValue1.Directives.NotNull().Count(0);
        var enumValue2 = enums.EnumValues[1];
        Assert.Equal(string.Empty, enumValue2.Description);
        Assert.Equal("BUZZ", enumValue2.Name);
        enumValue2.Directives.NotNull().Count(0);
    }

    [Fact]
    public void ThreeEnums()
    {
        var t = new Core.Parser("enum foo { FIZZ BUZZ LAST }");
        var documentNode = t.Parse();

        var enums = documentNode.NotNull().EnumTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, enums.Description);
        Assert.Equal("foo", enums.Name);
        enums.EnumValues.NotNull().Count(3);
        var enumValue1 = enums.EnumValues[0];
        Assert.Equal(string.Empty, enumValue1.Description);
        Assert.Equal("FIZZ", enumValue1.Name);
        enumValue1.Directives.NotNull().Count(0);
        var enumValue2 = enums.EnumValues[1];
        Assert.Equal(string.Empty, enumValue2.Description);
        Assert.Equal("BUZZ", enumValue2.Name);
        enumValue2.Directives.NotNull().Count(0);
        var enumValue3 = enums.EnumValues[2];
        Assert.Equal(string.Empty, enumValue3.Description);
        Assert.Equal("LAST", enumValue3.Name);
        enumValue3.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" enum foo { \"fizz\" BUZZ }")]
    [InlineData("\"\"\"bar\"\"\" enum foo { \"\"\"fizz\"\"\"BUZZ }")]
    public void Description(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        var enums = documentNode.NotNull().EnumTypeDefinitions.NotNull().One();
        Assert.Equal("bar", enums.Description);
        Assert.Equal("foo", enums.Name);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal("fizz", enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
        enumValue.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var t = new Core.Parser("enum foo @bar { BUZZ @fizz }");
        var documentNode = t.Parse();

        var enums = documentNode.NotNull().EnumTypeDefinitions.NotNull().One();
        Assert.Equal(string.Empty, enums.Description);
        Assert.Equal("foo", enums.Name);
        var typeDirective = enums.Directives.NotNull().One();
        Assert.Equal("bar", typeDirective.Name);
        typeDirective.Arguments.NotNull().Count(0);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal(string.Empty, enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
        var valueDirective = enumValue.Directives.NotNull().One();
        Assert.Equal("fizz", valueDirective.Name);
        valueDirective.Arguments.NotNull().Count(0);
    }

    [Theory]
    [InlineData("enum")]
    [InlineData("enum foo {")]
    [InlineData("enum foo { FIZZ ")]
    [InlineData("enum foo { FIZZ BUZZ")]
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



