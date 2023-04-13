namespace RocketQL.Core.UnitTests.GraphQLParser;

public class ExtendUnionTypeDefinition
{
    [Theory]
    [InlineData("extend union foo = bar")]
    [InlineData("extend union foo = | bar")]
    public void OneMember(string schema)
    {
        var t = new Core.GraphQLParser(schema);
        var documentNode = t.Parse();

        var union = documentNode.NotNull().ExtendUnionTypes.NotNull().One();
        Assert.Equal("foo", union.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("bar", member);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("extend union foo = bar | fizz")]
    [InlineData("extend union foo = | bar | fizz")]
    public void TwoMembers(string schema)
    {
        var t = new Core.GraphQLParser(schema);
        var documentNode = t.Parse();

        var union = documentNode.NotNull().ExtendUnionTypes.NotNull().One();
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(2);
        Assert.Equal("bar", union.MemberTypes[0]);
        Assert.Equal("fizz", union.MemberTypes[1]);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("extend union foo = bar | fizz | hello")]
    [InlineData("extend union foo = | bar | fizz | hello")]
    public void ThreeMembers(string schema)
    {
        var t = new Core.GraphQLParser(schema);
        var documentNode = t.Parse();

        var union = documentNode.NotNull().ExtendUnionTypes.NotNull().One();
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(3);
        Assert.Equal("bar", union.MemberTypes[0]);
        Assert.Equal("fizz", union.MemberTypes[1]);
        Assert.Equal("hello", union.MemberTypes[2]);
        union.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var t = new Core.GraphQLParser("extend union foo @bar = fizz");
        var documentNode = t.Parse();

        var union = documentNode.NotNull().ExtendUnionTypes.NotNull().One();
        Assert.Equal("foo", union.Name);
        var directive = union.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("fizz", member);
    }

    [Theory]
    [InlineData("extend union")]
    [InlineData("extend union foo = ")]
    [InlineData("extend union foo = bar |")]
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

    [Fact]
    public void ExtendUnionTypeMissingAtLeastOne()
    {
        var t = new Core.GraphQLParser("extend union foo 42");
        try
        {
            var documentNode = t.Parse();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend union must specify at least one of directive or member types.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



