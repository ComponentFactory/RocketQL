namespace RocketQL.Core.UnitTests.Parser;

public class Directive
{
    [Fact]
    public void OneNoArguments()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("bar", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(node.DefaultValue);
        Assert.Single(node.Directives);
        DirectiveNode directiveNode = node.Directives[0];
        Assert.Equal("hello", directiveNode.Name);
        Assert.Empty(directiveNode.Arguments);
    }

    [Fact]
    public void OneWithOneArgument()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello (world: 3)) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("bar", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(node.DefaultValue);
        Assert.Single(node.Directives);
        DirectiveNode directiveNode = node.Directives[0];
        Assert.Equal("hello", directiveNode.Name);
        Assert.Single(directiveNode.Arguments);
        ObjectFieldNode argument = directiveNode.Arguments[0];
        Assert.Equal("world", argument.Name);
        Assert.IsType<IntValueNode>(argument.Value);
        IntValueNode argumentValue = (IntValueNode)argument.Value;
        Assert.Equal("3", argumentValue.Value);
    }

    [Fact]
    public void OneWithTwoArguments()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello (world: 3, second: true)) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("bar", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(node.DefaultValue);
        Assert.Single(node.Directives);
        DirectiveNode directiveNode = node.Directives[0];
        Assert.Equal("hello", directiveNode.Name);
        Assert.Equal(2, directiveNode.Arguments.Count);
        ObjectFieldNode argument1 = directiveNode.Arguments[0];
        Assert.Equal("world", argument1.Name);
        Assert.IsType<IntValueNode>(argument1.Value);
        IntValueNode argument1Value = (IntValueNode)argument1.Value;
        Assert.Equal("3", argument1Value.Value);
        ObjectFieldNode argument2 = directiveNode.Arguments[1];
        Assert.Equal("second", argument2.Name);
        Assert.IsType<BooleanValueNode>(argument2.Value);
        BooleanValueNode argument2Value = (BooleanValueNode)argument2.Value;
        Assert.True(argument2Value.Value);
    }

    [Fact]
    public void TwoNoArguments()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello @world) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("bar", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(node.DefaultValue);
        Assert.Equal(2, node.Directives.Count);
        DirectiveNode directiveNode1 = node.Directives[0];
        Assert.Equal("hello", directiveNode1.Name);
        Assert.Empty(directiveNode1.Arguments);
        DirectiveNode directiveNode2 = node.Directives[1];
        Assert.Equal("world", directiveNode2.Name);
        Assert.Empty(directiveNode2.Arguments);
    }

    [Theory]
    [InlineData("directive @foo (bar: fizz @")]
    [InlineData("directive @foo (bar: fizz @hello")]
    [InlineData("directive @foo (bar: fizz @hello (")]
    [InlineData("directive @foo (bar: fizz @hello (world")]
    [InlineData("directive @foo (bar: fizz @hello (world:")]
    [InlineData("directive @foo (bar: fizz @hello (world: 3")]
    public void UnexpectedEndOfFile(string text)
    {
        var t = new Core.Parser(text);
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

    [Theory]
    [InlineData("directive @foo (bar: fizz @42", TokenKind.Name, TokenKind.IntValue)]
    [InlineData("directive @foo (bar: fizz @hello 42", TokenKind.RightParenthesis, TokenKind.IntValue)]
    [InlineData("directive @foo (bar: fizz @hello (42", TokenKind.Name, TokenKind.IntValue)]
    [InlineData("directive @foo (bar: fizz @hello (world 42", TokenKind.Colon, TokenKind.IntValue)]
    [InlineData("directive @foo (bar: fizz @hello (world: 42 43", TokenKind.Name, TokenKind.IntValue)]
    public void ExpectedTokenNotFound(string text, TokenKind expected, TokenKind found)
    {
        var t = new Core.Parser(text);
        try
        {
            var documentNode = t.Parse();
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

