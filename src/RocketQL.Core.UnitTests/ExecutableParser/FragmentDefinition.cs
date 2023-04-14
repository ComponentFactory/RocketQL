namespace RocketQL.Core.UnitTests.ExecutableParser;

public class FragmentDefinition
{
    [Fact]
    public void Minimal()
    {
        var t = new Core.ExecutableParser("fragment foo on bar { fizz }");
        var documentNode = t.Parse();

        var fragment = documentNode.NotNull().Fragments.NotNull().One();
        Assert.Equal("foo", fragment.Name);
        Assert.Equal("bar", fragment.TypeCondition);
        fragment.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)fragment.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("fizz", field.Name);
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);
    }

    [Fact]
    public void FragmentWithDirective()
    {
        var t = new Core.ExecutableParser("fragment foo on bar @buzz { fizz }");
        var documentNode = t.Parse();

        var fragment = documentNode.NotNull().Fragments.NotNull().One();
        Assert.Equal("foo", fragment.Name);
        Assert.Equal("bar", fragment.TypeCondition);
        var directive = fragment.Directives.NotNull().One();
        Assert.Equal("buzz", directive.Name);
        var field = (FieldSelectionNode)fragment.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("fizz", field.Name);
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);
    }

    [Theory]
    [InlineData("fragment")]
    [InlineData("fragment foo")]
    [InlineData("fragment foo on")]
    [InlineData("fragment foo on bar")]
    [InlineData("fragment foo on bar {")]
    [InlineData("fragment foo on bar { fizz")]
    public void UnexpectedEndOfFile(string text)
    {
        var t = new Core.ExecutableParser(text);
        try
        {
            var documentNode = t.Parse();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal("Unexpected end of file encountered.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void FragmentNameCannotBeOn()
    {
        var t = new Core.ExecutableParser("fragment on");
        try
        {
            var documentNode = t.Parse();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal("Fragment name cannot be the keyword 'on'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



