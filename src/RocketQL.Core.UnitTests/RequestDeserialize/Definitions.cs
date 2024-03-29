﻿namespace RocketQL.Core.UnitTests.RequestDeserialize;

public class Definitions : UnitTestBase
{
    [Theory]
    [InlineData("schema")]
    [InlineData("scalar")]
    [InlineData("type")]
    [InlineData("interface")]
    [InlineData("union")]
    [InlineData("enum")]
    [InlineData("input")]
    [InlineData("directive")]
    public void DefinintionNotAllowedInOperation(string text)
    {
        try
        {
            var documentNode = Serialization.RequestDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Request cannot contain definition '{text}'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("extend schema", "schema")]
    [InlineData("extend scalar", "scalar")]
    [InlineData("extend type", "type")]
    [InlineData("extend interface", "interface")]
    [InlineData("extend union", "union")]
    [InlineData("extend enum", "enum")]
    [InlineData("extend input", "input")]
    public void ExtendDefinintionNotAllowedInOperation(string text, string definition)
    {
        try
        {
            var documentNode = Serialization.RequestDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Request cannot contain extend definition '{definition}'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}



