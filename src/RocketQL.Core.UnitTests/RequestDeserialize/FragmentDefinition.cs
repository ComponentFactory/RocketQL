using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;

namespace RocketQL.Core.UnitTests.RequestDeserialize;

public class FragmentDefinition
{
    [Fact]
    public void Minimal()
    {
        var documentNode = Document.RequestDeserialize("fragment foo on bar { fizz }");

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
        var documentNode = Document.RequestDeserialize("fragment foo on bar @buzz { fizz }");

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
        try
        {
            var documentNode = Document.RequestDeserialize(text);
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
        try
        {
            var documentNode = Document.RequestDeserialize("fragment on");
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



