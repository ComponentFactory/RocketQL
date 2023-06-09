﻿using RocketQL.Core.Serializers;

namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class ExtendScalarTypeDefinition
{
    [Fact]
    public void Minimum()
    {
        var documentNode = Document.SchemaDeserialize("extend scalar foo");

        var scalar = documentNode.NotNull().ExtendScalarTypes.NotNull().One();
        Assert.Equal("foo", scalar.Name);
        scalar.Directives.NotNull().Count(0);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Document.SchemaDeserialize("extend scalar foo @bar");

        var scalar = documentNode.NotNull().ExtendScalarTypes.NotNull().One();
        Assert.Equal("foo", scalar.Name);
        var directive = scalar.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
    }

    [Theory]
    [InlineData("extend")]
    [InlineData("extend scalar")]
    [InlineData("extend scalar foo @")]
    public void UnexpectedEndOfFile(string text)
    {
        try
        {
            var documentNode = Document.SchemaDeserialize(text);
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



