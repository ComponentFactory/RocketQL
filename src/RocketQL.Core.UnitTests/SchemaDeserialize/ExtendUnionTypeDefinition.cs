namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ExtendUnionTypeDefinition : UnitTestBase
{
    [Theory]
    [InlineData("extend union foo = bar")]
    [InlineData("extend union foo = | bar")]
    public void OneMember(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxExtendUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxExtendUnionTypeDefinitionNode)definition);
        Assert.Equal("foo", union.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("bar", member.Name);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("extend union foo = bar | fizz")]
    [InlineData("extend union foo = | bar | fizz")]
    public void TwoMembers(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxExtendUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxExtendUnionTypeDefinitionNode)definition);
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(2);
        Assert.Equal("bar", union.MemberTypes[0].Name);
        Assert.Equal("fizz", union.MemberTypes[1].Name);
        union.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("extend union foo = bar | fizz | hello")]
    [InlineData("extend union foo = | bar | fizz | hello")]
    public void ThreeMembers(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxExtendUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxExtendUnionTypeDefinitionNode)definition);
        Assert.Equal("foo", union.Name);
        union.MemberTypes.NotNull().Count(3);
        Assert.Equal("bar", union.MemberTypes[0].Name);
        Assert.Equal("fizz", union.MemberTypes[1].Name);
        Assert.Equal("hello", union.MemberTypes[2].Name);
        union.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("extend union foo @bar = fizz");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxExtendUnionTypeDefinitionNode>(definition);
        var union = ((SyntaxExtendUnionTypeDefinitionNode)definition);
        Assert.Equal("foo", union.Name);
        var directive = union.Directives.NotNull().One();
        Assert.Equal("@bar", directive.Name);
        var member = union.MemberTypes.NotNull().One();
        Assert.Equal("fizz", member.Name);
    }

    [Theory]
    [InlineData("extend union")]
    [InlineData("extend union foo = ")]
    [InlineData("extend union foo = bar |")]
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

    [Fact]
    public void ExtendUnionTypeMissingAtLeastOne()
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize("extend union foo 42");
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend union must specify at least one of directive or member type.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



