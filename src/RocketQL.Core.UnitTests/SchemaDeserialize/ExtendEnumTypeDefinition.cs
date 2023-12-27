namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ExtendEnumTypeDefinition : UnitTestBase
{
    [Fact]
    public void EnumValues()
    {
        var documentNode = Serialization.SchemaDeserialize("extend enum foo { BUZZ }");

        var enums = documentNode.NotNull().ExtendEnumTypes.NotNull().One();
        Assert.Equal("foo", enums.Name);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal(string.Empty, enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
        enumValue.Directives.NotNull().Count(0);
    }


    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("extend enum foo @bar");

        var enums = documentNode.NotNull().ExtendEnumTypes.NotNull().One();
        Assert.Equal("foo", enums.Name);
        var typeDirective = enums.Directives.NotNull().One();
        Assert.Equal("bar", typeDirective.Name);
        typeDirective.Arguments.NotNull().Count(0);
        enums.EnumValues.NotNull().Count(0);
    }

    [Fact]
    public void EnumValuesAndDirective()
    {
        var documentNode = Serialization.SchemaDeserialize("extend enum foo @bar { BUZZ }");

        var enums = documentNode.NotNull().ExtendEnumTypes.NotNull().One();
        Assert.Equal("foo", enums.Name);
        var typeDirective = enums.Directives.NotNull().One();
        Assert.Equal("bar", typeDirective.Name);
        typeDirective.Arguments.NotNull().Count(0);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal(string.Empty, enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
    }
    [Theory]
    [InlineData("extend enum")]
    [InlineData("extend enum foo {")]
    [InlineData("extend enum foo { FIZZ ")]
    [InlineData("extend enum foo { FIZZ BUZZ")]
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
    public void ExtendEnumTypeMissingAtLeastOne()
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize("extend enum foo 42");
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Extend enum must specify at least one of directive or enum values.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



