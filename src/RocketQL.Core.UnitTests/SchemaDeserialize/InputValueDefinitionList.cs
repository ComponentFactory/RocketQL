﻿using RocketQL.Core.Nodes;
using RocketQL.Core.Tokenizers;
using RocketQL.Core.Serializers;

namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class InputValueDefinitionList
{
    [Theory]
    [InlineData("directive @foo (fizz: buzz) on ENUM", false)]
    [InlineData("directive @foo (fizz: buzz!) on ENUM", true)]
    public void SingleNameType(string schema, bool nonNull)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, argument.Description);
        Assert.Equal("fizz", argument.Name);
        Assert.IsType<TypeNameNode>(argument.Type);
        TypeNameNode nameNode = (TypeNameNode)argument.Type;
        Assert.Equal("buzz", nameNode.Name);
        Assert.Equal(nonNull, nameNode.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (fizz: [buzz]) on ENUM", false, false)]
    [InlineData("directive @foo (fizz: [buzz]!) on ENUM", true, false)]
    [InlineData("directive @foo (fizz: [buzz!]) on ENUM", false, true)]
    [InlineData("directive @foo (fizz: [buzz!]!) on ENUM", true, true)]
    public void SingleListType(string schema, bool listNonNull, bool typeNonNull)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, argument.Description);
        Assert.Equal("fizz", argument.Name);
        Assert.IsType<TypeListNode>(argument.Type);
        TypeListNode listNode = (TypeListNode)argument.Type;
        Assert.Equal(listNonNull, listNode.NonNull);
        Assert.IsType<TypeNameNode>(listNode.Type);
        TypeNameNode nameNode = (TypeNameNode)listNode.Type;
        Assert.Equal("buzz", nameNode.Name);
        Assert.Equal(typeNonNull, nameNode.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (fizz: [[buzz]]) on ENUM", false, false, false)]
    [InlineData("directive @foo (fizz: [[buzz]]!) on ENUM", true, false, false)]
    [InlineData("directive @foo (fizz: [[buzz]!]) on ENUM", false, true, false)]
    [InlineData("directive @foo (fizz: [[buzz]!]!) on ENUM", true, true, false)]
    [InlineData("directive @foo (fizz: [[buzz!]]) on ENUM", false, false, true)]
    [InlineData("directive @foo (fizz: [[buzz!]]!) on ENUM", true, false, true)]
    [InlineData("directive @foo (fizz: [[buzz!]!]) on ENUM", false, true, true)]
    [InlineData("directive @foo (fizz: [[buzz!]!]!) on ENUM", true, true, true)]
    public void SingleListListType(string schema, bool outerNonNull, bool innerNonNull, bool typeNonNull)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, argument.Description);
        Assert.Equal("fizz", argument.Name);
        Assert.IsType<TypeListNode>(argument.Type);
        TypeListNode listNodeOuter = (TypeListNode)argument.Type;
        Assert.Equal(outerNonNull, listNodeOuter.NonNull);
        Assert.IsType<TypeListNode>(listNodeOuter.Type);
        TypeListNode listNodeInner = (TypeListNode)listNodeOuter.Type;
        Assert.Equal(innerNonNull, listNodeInner.NonNull);
        Assert.IsType<TypeNameNode>(listNodeInner.Type);
        TypeNameNode nameNode = (TypeNameNode)listNodeInner.Type;
        Assert.Equal("buzz", nameNode.Name);
        Assert.Equal(typeNonNull, nameNode.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (fizz:buzz hello:world) on ENUM")]
    [InlineData("directive @foo (fizz: buzz hello: world) on ENUM")]
    [InlineData("directive @foo (fizz:buzz,hello: world) on ENUM")]
    [InlineData("directive @foo (fizz: buzz, hello:world) on ENUM")]
    public void DoubleNameType(string schema)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var arguments = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().Count(2);
        var argument1 = arguments[0];
        Assert.Equal(string.Empty, argument1.Description);
        Assert.Equal("fizz", argument1.Name);
        Assert.IsType<TypeNameNode>(argument1.Type);
        TypeNameNode nameNode1 = (TypeNameNode)argument1.Type;
        Assert.Equal("buzz", nameNode1.Name);
        Assert.False(nameNode1.NonNull);

        var argument2 = arguments[1];
        Assert.Equal(string.Empty, argument2.Description);
        Assert.Equal("hello", argument2.Name);
        Assert.IsType<TypeNameNode>(argument2.Type);
        TypeNameNode nameNode2 = (TypeNameNode)argument2.Type;
        Assert.Equal("world", nameNode2.Name);
        Assert.False(nameNode2.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (bar")]
    [InlineData("directive @foo (bar:")]
    [InlineData("directive @foo (bar: fizz")]
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

    [Theory]
    [InlineData("directive @foo (42", DocumentTokenKind.Name, DocumentTokenKind.IntValue)]
    [InlineData("directive @foo (bar 42", DocumentTokenKind.Colon, DocumentTokenKind.IntValue)]
    [InlineData("directive @foo (bar: fizz 42", DocumentTokenKind.RightParenthesis, DocumentTokenKind.IntValue)]
    public void ExpectedTokenNotFound(string text, DocumentTokenKind expected, DocumentTokenKind found)
    {
        try
        {
            var documentNode = Document.SchemaDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Expected token '{expected}' but found '{found}' instead.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

