﻿namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class EnumTypeDefinition : UnitTestBase
{
    [Theory]
    [InlineData("enum foo")]
    [InlineData("enum foo { }")]
    public void Minimum(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxEnumTypeDefinitionNode>(definition);
        var enums = ((SyntaxEnumTypeDefinitionNode)definition);
        Assert.Equal("", enums.Description);
        Assert.Equal("foo", enums.Name);
        enums.EnumValues.NotNull().Count(0);
    }

    [Fact]
    public void OneEnum()
    {
        var documentNode = Serialization.SchemaDeserialize("enum foo { BUZZ }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxEnumTypeDefinitionNode>(definition);
        var enums = ((SyntaxEnumTypeDefinitionNode)definition);
        Assert.Equal("", enums.Description);
        Assert.Equal("foo", enums.Name);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal("", enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
        enumValue.Directives.NotNull().Count(0);
    }

    [Fact]
    public void TwoEnums()
    {
        var documentNode = Serialization.SchemaDeserialize("enum foo { FIZZ BUZZ }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxEnumTypeDefinitionNode>(definition);
        var enums = ((SyntaxEnumTypeDefinitionNode)definition);
        Assert.Equal("", enums.Description);
        Assert.Equal("foo", enums.Name);
        enums.EnumValues.NotNull().Count(2);
        var enumValue1 = enums.EnumValues[0];
        Assert.Equal("", enumValue1.Description);
        Assert.Equal("FIZZ", enumValue1.Name);
        enumValue1.Directives.NotNull().Count(0);
        var enumValue2 = enums.EnumValues[1];
        Assert.Equal("", enumValue2.Description);
        Assert.Equal("BUZZ", enumValue2.Name);
        enumValue2.Directives.NotNull().Count(0);
    }

    [Fact]
    public void ThreeEnums()
    {
        var documentNode = Serialization.SchemaDeserialize("enum foo { FIZZ BUZZ LAST }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxEnumTypeDefinitionNode>(definition);
        var enums = ((SyntaxEnumTypeDefinitionNode)definition);
        Assert.Equal("", enums.Description);
        Assert.Equal("foo", enums.Name);
        enums.EnumValues.NotNull().Count(3);
        var enumValue1 = enums.EnumValues[0];
        Assert.Equal("", enumValue1.Description);
        Assert.Equal("FIZZ", enumValue1.Name);
        enumValue1.Directives.NotNull().Count(0);
        var enumValue2 = enums.EnumValues[1];
        Assert.Equal("", enumValue2.Description);
        Assert.Equal("BUZZ", enumValue2.Name);
        enumValue2.Directives.NotNull().Count(0);
        var enumValue3 = enums.EnumValues[2];
        Assert.Equal("", enumValue3.Description);
        Assert.Equal("LAST", enumValue3.Name);
        enumValue3.Directives.NotNull().Count(0);
    }

    [Theory]
    [InlineData("\"bar\" enum foo { \"fizz\" BUZZ }")]
    [InlineData("\"\"\"bar\"\"\" enum foo { \"\"\"fizz\"\"\"BUZZ }")]
    public void Description(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxEnumTypeDefinitionNode>(definition);
        var enums = ((SyntaxEnumTypeDefinitionNode)definition);
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
        var documentNode = Serialization.SchemaDeserialize("enum foo @bar { BUZZ @fizz }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxEnumTypeDefinitionNode>(definition);
        var enums = ((SyntaxEnumTypeDefinitionNode)definition);
        Assert.Equal("", enums.Description);
        Assert.Equal("foo", enums.Name);
        var typeDirective = enums.Directives.NotNull().One();
        Assert.Equal("@bar", typeDirective.Name);
        typeDirective.Arguments.NotNull().Count(0);
        var enumValue = enums.EnumValues.NotNull().One();
        Assert.Equal("", enumValue.Description);
        Assert.Equal("BUZZ", enumValue.Name);
        var valueDirective = enumValue.Directives.NotNull().One();
        Assert.Equal("@fizz", valueDirective.Name);
        valueDirective.Arguments.NotNull().Count(0);
    }

    [Theory]
    [InlineData("enum")]
    [InlineData("enum foo {")]
    [InlineData("enum foo { FIZZ ")]
    [InlineData("enum foo { FIZZ BUZZ")]
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



