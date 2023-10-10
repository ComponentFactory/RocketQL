namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class UnionTypeDefinition
{
    [Theory]
    [InlineData("union foo = bar")]
    [InlineData("union foo = | bar")]
    public void OneMember(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize("test", schema);

        var union = documentNode.NotNull().UnionTypes.NotNull().One();
        Assert.Equal(string.Empty, union.Description);
        Assert.Equal("foo", union.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("bar", member);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("union foo = bar | fizz")]
    [InlineData("union foo = | bar | fizz")]
    public void TwoMembers(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize("test", schema);

        var union = documentNode.NotNull().UnionTypes.NotNull().One();
        Assert.Equal(string.Empty, union.Description);
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(2);
        Assert.Equal("bar", union.MemberTypes[0]);
        Assert.Equal("fizz", union.MemberTypes[1]);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("union foo = bar | fizz | hello")]
    [InlineData("union foo = | bar | fizz | hello")]
    public void ThreeMembers(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize("test", schema);

        var union = documentNode.NotNull().UnionTypes.NotNull().One();
        Assert.Equal(string.Empty, union.Description);
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(3);
        Assert.Equal("bar", union.MemberTypes[0]);
        Assert.Equal("fizz", union.MemberTypes[1]);
        Assert.Equal("hello", union.MemberTypes[2]);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" union foo = bar")]
    [InlineData("\"\"\"bar\"\"\" union foo = bar")]
    public void Description(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize("test", schema);

        var union = documentNode.NotNull().UnionTypes.NotNull().One();
        Assert.Equal("bar", union.Description);
        Assert.Equal("foo", union.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("bar", member);
        union.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("test", "union foo @bar = fizz");

        var union = documentNode.NotNull().UnionTypes.NotNull().One();
        Assert.Equal(string.Empty, union.Description);
        Assert.Equal("foo", union.Name);
        var directive = union.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("fizz", member);
    }

    [Theory]
    [InlineData("union")]
    [InlineData("union foo")]
    [InlineData("union foo = ")]
    [InlineData("union foo = bar |")]
    public void UnexpectedEndOfFile(string text)
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize("test", text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal("test", ex.Locations[0].Source);
            Assert.Equal($"Unexpected end of file encountered.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



