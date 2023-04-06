namespace RocketQL.Core.UnitTests.Parser;

public class Value
{
    [Fact]
    public void IntValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = 1) on ENUM");
        var documentNode = t.Parse();
        
        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<IntValueNode>(node.DefaultValue);
        IntValueNode valueNode = (IntValueNode)node.DefaultValue;
        Assert.Equal("1", valueNode.Value);
    }

    [Fact]
    public void FloatValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = 3.14159) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<FloatValueNode>(node.DefaultValue);
        FloatValueNode valueNode = (FloatValueNode)node.DefaultValue;
        Assert.Equal("3.14159", valueNode.Value);
    }

    [Fact]
    public void StringValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = \"word\") on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<StringValueNode>(node.DefaultValue);
        StringValueNode valueNode = (StringValueNode)node.DefaultValue;
        Assert.Equal("word", valueNode.Value);
    }

    [Fact]
    public void StringValueFromBlockString()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = \"\"\"word\"\"\") on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<StringValueNode>(node.DefaultValue);
        StringValueNode valueNode = (StringValueNode)node.DefaultValue;
        Assert.Equal("word", valueNode.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BooleanValue(bool value)
    {
        var t = new Core.Parser($"directive @foo (fizz: buzz = {value.ToString().ToLower()}) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<BooleanValueNode>(node.DefaultValue);
        BooleanValueNode valueNode = (BooleanValueNode)node.DefaultValue;
        Assert.Equal(value, valueNode.Value);
    }

    [Fact]
    public void NullValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = null) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<NullValueNode>(node.DefaultValue);
    }

    [Fact]
    public void EnumValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = ORANGE) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<EnumValueNode>(node.DefaultValue);
        EnumValueNode valueNode = (EnumValueNode)node.DefaultValue;
        Assert.Equal("ORANGE", valueNode.Value);
    }

    [Fact]
    public void ListValueEmpty()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = []) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ListValueNode>(node.DefaultValue);
        ListValueNode valueNode = (ListValueNode)node.DefaultValue;
        Assert.Empty(valueNode.Values);
    }

    [Fact]
    public void ListValueOneEntry()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = [3]) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ListValueNode>(node.DefaultValue);
        ListValueNode valueNode = (ListValueNode)node.DefaultValue;
        Assert.Single(valueNode.Values);
        Assert.IsType<IntValueNode>(valueNode.Values[0]);
        IntValueNode entryNode = (IntValueNode)valueNode.Values[0];
        Assert.Equal("3", entryNode.Value);
    }

    [Theory]
    [InlineData("directive @foo (fizz: buzz = [3,true,null]) on ENUM")]
    [InlineData("directive @foo (fizz: buzz = [3 true null]) on ENUM")]
    [InlineData("directive @foo (fizz: buzz = [3, true null ]) on ENUM")]
    public void ListValueThreeEntries(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ListValueNode>(node.DefaultValue);
        ListValueNode valueNode = (ListValueNode)node.DefaultValue;
        Assert.Equal(3, valueNode.Values.Count);
        Assert.IsType<IntValueNode>(valueNode.Values[0]);
        IntValueNode entryNode1 = (IntValueNode)valueNode.Values[0];
        Assert.Equal("3", entryNode1.Value);
        Assert.IsType<BooleanValueNode>(valueNode.Values[1]);
        BooleanValueNode entryNode2 = (BooleanValueNode)valueNode.Values[1];
        Assert.True(entryNode2.Value);
        Assert.IsType<NullValueNode>(valueNode.Values[2]);
    }

    [Fact]
    public void ListValueListValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = [3 [4]]) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ListValueNode>(node.DefaultValue);
        ListValueNode valueNode = (ListValueNode)node.DefaultValue;
        Assert.Equal(2, valueNode.Values.Count);
        Assert.IsType<IntValueNode>(valueNode.Values[0]);
        IntValueNode entryNode1 = (IntValueNode)valueNode.Values[0];
        Assert.Equal("3", entryNode1.Value);
        Assert.IsType<ListValueNode>(valueNode.Values[1]);
        ListValueNode innerNode = (ListValueNode)valueNode.Values[1];
        Assert.Single(innerNode.Values);
        Assert.IsType<IntValueNode>(innerNode.Values[0]);
        IntValueNode entryNode2 = (IntValueNode)innerNode.Values[0];
        Assert.Equal("4", entryNode2.Value);
    }

    [Fact]
    public void ListValueObjectValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = [3 { hello: null }]) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ListValueNode>(node.DefaultValue);
        ListValueNode valueNode = (ListValueNode)node.DefaultValue;
        Assert.Equal(2, valueNode.Values.Count);
        Assert.IsType<IntValueNode>(valueNode.Values[0]);
        IntValueNode entryNode1 = (IntValueNode)valueNode.Values[0];
        Assert.Equal("3", entryNode1.Value);
        Assert.IsType<ObjectValueNode>(valueNode.Values[1]);
        ObjectValueNode innerNode = (ObjectValueNode)valueNode.Values[1];
        Assert.Single(innerNode.ObjectFields);
        ObjectFieldNode fieldNode = innerNode.ObjectFields[0];
        Assert.Equal("hello", fieldNode.Name);
        Assert.IsType<NullValueNode>(fieldNode.Value);
    }

    [Fact]
    public void ObjectValueEmpty()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = {}) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ObjectValueNode>(node.DefaultValue);
        ObjectValueNode valueNode = (ObjectValueNode)node.DefaultValue;
        Assert.Empty(valueNode.ObjectFields);
    }

    [Fact]
    public void ObjectValueOneEntry()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = { world: 42 }) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ObjectValueNode>(node.DefaultValue);
        ObjectValueNode valueNode = (ObjectValueNode)node.DefaultValue;
        Assert.Single(valueNode.ObjectFields);
        ObjectFieldNode fieldNode = valueNode.ObjectFields[0];
        Assert.Equal("world", fieldNode.Name);
        Assert.IsType<IntValueNode>(fieldNode.Value);
        IntValueNode intNode = (IntValueNode)fieldNode.Value;
        Assert.Equal("42", intNode.Value);
    }

    [Fact]
    public void ObjectValueObjectValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = { world: { hello: 42 } }) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ObjectValueNode>(node.DefaultValue);
        ObjectValueNode valueNode1 = (ObjectValueNode)node.DefaultValue;
        Assert.Single(valueNode1.ObjectFields);
        ObjectFieldNode fieldNode1 = valueNode1.ObjectFields[0];
        Assert.Equal("world", fieldNode1.Name);
        Assert.IsType<ObjectValueNode>(fieldNode1.Value);
        ObjectValueNode valueNode2 = (ObjectValueNode)fieldNode1.Value;
        Assert.Single(valueNode2.ObjectFields);
        ObjectFieldNode fieldNode2 = valueNode2.ObjectFields[0];
        Assert.Equal("hello", fieldNode2.Name);
        Assert.IsType<IntValueNode>(fieldNode2.Value);
        IntValueNode intNode = (IntValueNode)fieldNode2.Value;
        Assert.Equal("42", intNode.Value);
    }

    [Fact]
    public void ObjectValueListValue()
    {
        var t = new Core.Parser("directive @foo (fizz: buzz = { world: [42] }) on ENUM");
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.NotNull(node.DefaultValue);
        Assert.IsType<ObjectValueNode>(node.DefaultValue);
        ObjectValueNode valueNode1 = (ObjectValueNode)node.DefaultValue;
        Assert.Single(valueNode1.ObjectFields);
        ObjectFieldNode fieldNode1 = valueNode1.ObjectFields[0];
        Assert.Equal("world", fieldNode1.Name);
        Assert.IsType<ListValueNode>(fieldNode1.Value);
        ListValueNode innerNode = (ListValueNode)fieldNode1.Value;
        Assert.Single(innerNode.Values);
        Assert.IsType<IntValueNode>(innerNode.Values[0]);
        IntValueNode entryNode2 = (IntValueNode)innerNode.Values[0];
        Assert.Equal("42", entryNode2.Value);
    }

    [Theory]
    [InlineData("directive @foo (fizz: buzz = { world: $ }) on ENUM", TokenKind.Dollar)]
    [InlineData("directive @foo (fizz: buzz = { world: @ }) on ENUM", TokenKind.At)]
    [InlineData("directive @foo (fizz: buzz = { world: : }) on ENUM", TokenKind.Colon)]
    [InlineData("directive @foo (fizz: buzz = { world: ... }) on ENUM", TokenKind.Spread)]
    public void TokenNotAllowedHere(string text, TokenKind tokenKind)
    {
        var t = new Core.Parser(text);
        try
        {
            var documentNode = t.Parse();
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Token '{tokenKind}' not allowed in this position.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

