using RocketQL.Core.Nodes;
using RocketQL.Core.Tokenizers;
using RocketQL.Core.Serializers;

namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class DirectiveDefinition
{
    [Theory]
    [InlineData("directive @foo on ENUM")]
    [InlineData("directive @ foo on ENUM")]
    [InlineData("  directive, @ foo, on ,  ENUM")]
    public void Minimum(string schema)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        directive.Arguments.NotNull().Count(0);
        Assert.False(directive.Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, directive.DirectiveLocations);
    }

    [Theory]
    [InlineData(DirectiveLocations.QUERY)]
    [InlineData(DirectiveLocations.MUTATION)]
    [InlineData(DirectiveLocations.SUBSCRIPTION)]
    [InlineData(DirectiveLocations.FIELD)]
    [InlineData(DirectiveLocations.FRAGMENT_DEFINITION)]
    [InlineData(DirectiveLocations.FRAGMENT_SPREAD)]
    [InlineData(DirectiveLocations.INLINE_FRAGMENT)]
    [InlineData(DirectiveLocations.VARIABLE_DEFINITION)]
    [InlineData(DirectiveLocations.SCHEMA)]
    [InlineData(DirectiveLocations.SCALAR)]
    [InlineData(DirectiveLocations.OBJECT)]
    [InlineData(DirectiveLocations.FIELD_DEFINITION)]
    [InlineData(DirectiveLocations.ARGUMENT_DEFINITION)]
    [InlineData(DirectiveLocations.INTERFACE)]
    [InlineData(DirectiveLocations.UNION)]
    [InlineData(DirectiveLocations.ENUM)]
    [InlineData(DirectiveLocations.ENUM_VALUE)]
    [InlineData(DirectiveLocations.INPUT_OBJECT)]
    [InlineData(DirectiveLocations.INPUT_FIELD_DEFINITION)]
    public void SingleLocation(DirectiveLocations location)
    {
        var documentNode = Document.SchemaDeserialize($"directive @foo on {location}");

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        directive.Arguments.NotNull().Count(0);
        Assert.False(directive.Repeatable);
        Assert.Equal(location, directive.DirectiveLocations);
    }

    [Theory]
    [InlineData(DirectiveLocations.QUERY | DirectiveLocations.MUTATION | DirectiveLocations.SUBSCRIPTION, "QUERY | MUTATION | SUBSCRIPTION")]
    [InlineData(DirectiveLocations.FRAGMENT_DEFINITION | DirectiveLocations.FRAGMENT_SPREAD | DirectiveLocations.INLINE_FRAGMENT, "FRAGMENT_DEFINITION | FRAGMENT_SPREAD | INLINE_FRAGMENT")]
    [InlineData(DirectiveLocations.SCHEMA | DirectiveLocations.SCALAR | DirectiveLocations.OBJECT, "SCHEMA | SCALAR | OBJECT")]
    [InlineData(DirectiveLocations.FIELD_DEFINITION | DirectiveLocations.ARGUMENT_DEFINITION | DirectiveLocations.INTERFACE, "FIELD_DEFINITION | ARGUMENT_DEFINITION | INTERFACE")]
    [InlineData(DirectiveLocations.UNION | DirectiveLocations.ENUM | DirectiveLocations.ENUM_VALUE, "UNION | ENUM | ENUM_VALUE")]
    [InlineData(DirectiveLocations.INPUT_OBJECT | DirectiveLocations.INPUT_FIELD_DEFINITION, "INPUT_OBJECT | INPUT_FIELD_DEFINITION")]
    [InlineData(DirectiveLocations.EXECUTABLE_DRECTIVE_LOCATIONS, "QUERY | MUTATION | SUBSCRIPTION | FIELD | FRAGMENT_DEFINITION | FRAGMENT_SPREAD | INLINE_FRAGMENT | VARIABLE_DEFINITION")]
    [InlineData(DirectiveLocations.TYPE_SYSTEM_DRECTIVE_LOCATIONS, "SCHEMA | SCALAR | OBJECT | FIELD_DEFINITION | ARGUMENT_DEFINITION | INTERFACE | UNION | ENUM | ENUM_VALUE | INPUT_OBJECT | INPUT_FIELD_DEFINITION")]
    [InlineData(DirectiveLocations.EXECUTABLE_DRECTIVE_LOCATIONS | DirectiveLocations.TYPE_SYSTEM_DRECTIVE_LOCATIONS, "QUERY | MUTATION | SUBSCRIPTION | FIELD | FRAGMENT_DEFINITION | FRAGMENT_SPREAD | INLINE_FRAGMENT | VARIABLE_DEFINITION | SCHEMA | SCALAR | OBJECT | FIELD_DEFINITION | ARGUMENT_DEFINITION | INTERFACE | UNION | ENUM | ENUM_VALUE | INPUT_OBJECT | INPUT_FIELD_DEFINITION")]
    public void MultipleLocations(DirectiveLocations location, string str)
    {
        var documentNode = Document.SchemaDeserialize($"directive @foo on {str}");

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        directive.Arguments.NotNull().Count(0);
        Assert.False(directive.Repeatable);
        Assert.Equal(location, directive.DirectiveLocations);
    }

    [Theory]
    [InlineData("directive @foo on ENUM", false)]
    [InlineData("directive @foo repeatable on ENUM", true)]
    public void Repeatable(string schema, bool repeatable)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        directive.Arguments.NotNull().Count(0);
        Assert.Equal(repeatable, directive.Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, directive.DirectiveLocations);
    }

    [Theory]
    [InlineData("\"bar\" directive @foo on ENUM")]
    [InlineData("\"\"\"bar\"\"\" directive @foo on ENUM")]
    public void Description(string schema)
    {
        var documentNode = Document.SchemaDeserialize(schema);

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal("bar", directive.Description);
        Assert.Equal("foo", directive.Name);
        directive.Arguments.NotNull().Count(0);
        Assert.False(directive.Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, directive.DirectiveLocations);
    }

    [Fact]
    public void SingleArgumentNameType()
    {
        var documentNode = Document.SchemaDeserialize("directive @foo (bar: fizz) on ENUM");

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        Assert.False(directive.Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, directive.DirectiveLocations);

        var argument = directive.Arguments.NotNull().One();
        Assert.Equal(string.Empty, argument.Description);
        Assert.Equal("bar", argument.Name);
        Assert.IsType<TypeNameNode>(argument.Type);
        TypeNameNode nameNode = (TypeNameNode)argument.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(argument.DefaultValue);
    }

    [Fact]
    public void SingleArgumentNameTypeWithDefault()
    {
        var documentNode = Document.SchemaDeserialize("directive @foo (bar: fizz = 3.14) on ENUM");

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        Assert.False(directive.Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, directive.DirectiveLocations);

        var argument = directive.Arguments.NotNull().One();
        Assert.Equal(string.Empty, argument.Description);
        Assert.Equal("bar", argument.Name);
        Assert.IsType<TypeNameNode>(argument.Type);
        TypeNameNode nameNode = (TypeNameNode)argument.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.NotNull(argument.DefaultValue);
        FloatValueNode floatValueNode = argument.DefaultValue.IsType<FloatValueNode>();
        Assert.Equal("3.14", floatValueNode.Value);
    }

    [Fact]
    public void ArgumentWithDirective()
    {
        var documentNode = Document.SchemaDeserialize("directive @foo (bar: fizz @hello) on ENUM");

        var directive = documentNode.NotNull().Directives.NotNull().One();
        Assert.Equal(string.Empty, directive.Description);
        Assert.Equal("foo", directive.Name);
        Assert.False(directive.Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, directive.DirectiveLocations);

        var argument = directive.Arguments.NotNull().One();
        Assert.Equal(string.Empty, argument.Description);
        Assert.Equal("bar", argument.Name);
        Assert.IsType<TypeNameNode>(argument.Type);
        TypeNameNode nameNode = (TypeNameNode)argument.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(argument.DefaultValue);
        DirectiveNode directiveNode = argument.Directives.NotNull().One();
        Assert.Equal("hello", directiveNode.Name);
        directiveNode.Arguments.NotNull().Count(0);
    }

    [Theory]
    [InlineData("directive")]
    [InlineData("directive @")]
    [InlineData("directive @foo")]
    [InlineData("directive @foo (")]
    [InlineData("directive @foo repeatable")]
    [InlineData("directive @foo (bar: fizz @hello) on")]
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
    [InlineData("directive foo", DocumentTokenKind.At, DocumentTokenKind.Name)]
    [InlineData("directive @42", DocumentTokenKind.Name, DocumentTokenKind.IntValue)]
    [InlineData("directive @foo repeatable 42", DocumentTokenKind.Name, DocumentTokenKind.IntValue)]
    [InlineData("directive @foo (bar: fizz @hello) on 42", DocumentTokenKind.Name, DocumentTokenKind.IntValue)]
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

    [Theory]
    [InlineData("directive @foo query", "query")]
    [InlineData("directive @foo repeatable query", "query")]
    [InlineData("directive @foo repeatable repeatable", "repeatable")]
    public void ExpectedKeywordNotFound(string text, string found)
    {
        try
        {
            var documentNode = Document.SchemaDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Expected keyword 'on' but found '{found}' instead.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

