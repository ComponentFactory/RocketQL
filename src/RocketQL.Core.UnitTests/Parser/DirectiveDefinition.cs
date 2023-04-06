namespace RocketQL.Core.UnitTests.Parser;

public class DirectiveDefinition
{
    [Theory]
    [InlineData("directive @foo on ENUM")]
    [InlineData("directive @ foo on ENUM")]
    [InlineData("  directive, @ foo, on ,  ENUM")]
    public void Minimum(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.Empty(documentNode.DirectiveDefinitions[0].Arguments);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, documentNode.DirectiveDefinitions[0].DirectiveLocations);
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
        var t = new Core.Parser($"directive @foo on {location}");
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.Empty(documentNode.DirectiveDefinitions[0].Arguments);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(location, documentNode.DirectiveDefinitions[0].DirectiveLocations);
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
        var t = new Core.Parser($"directive @foo on {str}");
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.Empty(documentNode.DirectiveDefinitions[0].Arguments);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(location, documentNode.DirectiveDefinitions[0].DirectiveLocations);
    }

    [Theory]
    [InlineData("directive @foo on ENUM", false)]
    [InlineData("directive @foo repeatable on ENUM", true)]
    public void Repeatable(string schema, bool repeatable)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.Empty(documentNode.DirectiveDefinitions[0].Arguments);
        Assert.Equal(repeatable, documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, documentNode.DirectiveDefinitions[0].DirectiveLocations);
    }

    [Theory]
    [InlineData("\"bar\" directive @foo on ENUM")]
    [InlineData("\"\"\"bar\"\"\" directive @foo on ENUM")]
    public void Description(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Equal("bar", documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.Empty(documentNode.DirectiveDefinitions[0].Arguments);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, documentNode.DirectiveDefinitions[0].DirectiveLocations);
    }

    [Fact]
    public void SingleArgumentNameType()
    {
        var t = new Core.Parser("directive @foo (bar: fizz) on ENUM");
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, documentNode.DirectiveDefinitions[0].DirectiveLocations);
        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("bar", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.Null(node.DefaultValue);
    }

    [Fact]
    public void SingleArgumentNameTypeWithDefault()
    {
        var t = new Core.Parser("directive @foo (bar: fizz = 3.14) on ENUM");
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, documentNode.DirectiveDefinitions[0].DirectiveLocations);
        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("bar", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("fizz", nameNode.Name);
        Assert.False(nameNode.NonNull);
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<FloatValueNode>(node.DefaultValue);
        FloatValueNode floatValueNode = (FloatValueNode)node.DefaultValue;
        Assert.Equal("3.14", floatValueNode.Value);
    }

    [Fact]
    public void ArgumentWithDirective()
    {
        var t = new Core.Parser("directive @foo (bar: fizz @hello) on ENUM");
        var documentNode = t.Parse();

        Assert.NotNull(documentNode);
        Assert.Single(documentNode.DirectiveDefinitions);
        Assert.Null(documentNode.DirectiveDefinitions[0].Description);
        Assert.Equal("foo", documentNode.DirectiveDefinitions[0].Name);
        Assert.False(documentNode.DirectiveDefinitions[0].Repeatable);
        Assert.Equal(DirectiveLocations.ENUM, documentNode.DirectiveDefinitions[0].DirectiveLocations);
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
}

