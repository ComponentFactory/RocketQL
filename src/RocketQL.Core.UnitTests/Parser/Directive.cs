namespace RocketQL.Core.UnitTests.Parser;

public class Directive
{
    [Fact]
    public void OneNoArguments()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello) on ENUM");
        var documentNode = t.Parse();

        var directive = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("bar", directive.Name);
        Assert.IsType<TypeNameNode>(directive.Type);
        TypeNameNode nameNode = (TypeNameNode)directive.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(directive.DefaultValue);
        Assert.Single(directive.Directives);
        DirectiveNode directiveNode = directive.Directives[0];
        Assert.Equal("hello", directiveNode.Name);
        directiveNode.Arguments.NotNull().Count(0);
    }

    [Fact]
    public void OneWithOneArgument()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello (world: 3)) on ENUM");
        var documentNode = t.Parse();

        var directive = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("bar", directive.Name);
        Assert.IsType<TypeNameNode>(directive.Type);
        TypeNameNode nameNode = (TypeNameNode)directive.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(directive.DefaultValue);
        DirectiveNode directiveNode = directive.Directives.NotNull().One();
        Assert.Equal("hello", directiveNode.Name);
        ObjectFieldNode argument = directiveNode.Arguments.NotNull().One();
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

        var directive = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("bar", directive.Name);
        Assert.IsType<TypeNameNode>(directive.Type);
        TypeNameNode nameNode = (TypeNameNode)directive.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(directive.DefaultValue);
        DirectiveNode directiveNode = directive.Directives.NotNull().One();
        Assert.Equal("hello", directiveNode.Name);
        directiveNode.Arguments.NotNull().Count(2);
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

        var directive = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("bar", directive.Name);
        Assert.IsType<TypeNameNode>(directive.Type);
        TypeNameNode nameNode = (TypeNameNode)directive.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(directive.DefaultValue);
        directive.Directives.NotNull().Count(2);
        DirectiveNode directiveNode1 = directive.Directives[0];
        Assert.Equal("hello", directiveNode1.Name);
        directiveNode1.Arguments.NotNull().Count(0);
        DirectiveNode directiveNode2 = directive.Directives[1];
        Assert.Equal("world", directiveNode2.Name);
        directiveNode2.Arguments.NotNull().Count(0);
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

