namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class UnionTypeDefinition : UnitTestBase
{
    [Theory]
    [InlineData("union foo = bar")]
    [InlineData("union foo = | bar")]
    public void OneMember(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxUnionTypeDefinitionNode)definition);
        Assert.Equal("", union.Description);
        Assert.Equal("foo", union.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("bar", member.Name);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("union foo = bar | fizz")]
    [InlineData("union foo = | bar | fizz")]
    public void TwoMembers(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxUnionTypeDefinitionNode)definition);
        Assert.Equal("", union.Description);
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(2);
        Assert.Equal("bar", union.MemberTypes[0].Name);
        Assert.Equal("fizz", union.MemberTypes[1].Name);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("union foo = bar | fizz | hello")]
    [InlineData("union foo = | bar | fizz | hello")]
    public void ThreeMembers(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxUnionTypeDefinitionNode)definition);
        Assert.Equal("", union.Description);
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(3);
        Assert.Equal("bar", union.MemberTypes[0].Name);
        Assert.Equal("fizz", union.MemberTypes[1].Name);
        Assert.Equal("hello", union.MemberTypes[2].Name);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" union foo = bar")]
    [InlineData("\"\"\"bar\"\"\" union foo = bar")]
    public void Description(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxUnionTypeDefinitionNode)definition);
        Assert.Equal("bar", union.Description);
        Assert.Equal("foo", union.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("bar", member.Name);
        union.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("union foo @bar = fizz");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxUnionTypeDefinitionNode)definition);
        Assert.Equal("", union.Description);
        Assert.Equal("foo", union.Name);
        var directive = union.Directives.NotNull().One();
        Assert.Equal("@bar", directive.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("fizz", member.Name);
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
}



