﻿namespace RocketQL.Core.UnitTests.TypeSystemParser;

public class ScalarTypeDefinition
{
    [Fact]
    public void Minimum()
    {
        var t = new Core.TypeSystemParser("scalar foo");
        var documentNode = t.Parse();

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal(string.Empty, scalar.Description);
        Assert.Equal("foo", scalar.Name);
        scalar.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" scalar foo")]
    [InlineData("\"\"\"bar\"\"\" scalar foo")]
    public void Description(string schema)
    {
        var t = new Core.TypeSystemParser(schema);
        var documentNode = t.Parse();

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal("bar", scalar.Description);
        Assert.Equal("foo", scalar.Name);
        scalar.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var t = new Core.TypeSystemParser("scalar foo @bar");
        var documentNode = t.Parse();

        var scalar = documentNode.NotNull().ScalarTypes.NotNull().One();
        Assert.Equal(string.Empty, scalar.Description);
        Assert.Equal("foo", scalar.Name);
        var directive = scalar.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
    }

    [Theory]
    [InlineData("scalar")]
    [InlineData("scalar foo @")]
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
}


