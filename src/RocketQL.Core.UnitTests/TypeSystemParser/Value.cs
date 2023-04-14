namespace RocketQL.Core.UnitTests.TypeSystemParser;

public class Value
{
    [Fact]
    public void IntValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = 1) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        IntValueNode valueNode = argument.DefaultValue.IsType<IntValueNode>();
        Assert.Equal("1", valueNode.Value);
    }

    [Fact]
    public void FloatValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = 3.14159) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        FloatValueNode valueNode = argument.DefaultValue.IsType<FloatValueNode>();
        Assert.Equal("3.14159", valueNode.Value);
    }

    [Fact]
    public void StringValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = \"word\") on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        StringValueNode valueNode = argument.DefaultValue.IsType<StringValueNode>();
        Assert.Equal("word", valueNode.Value);
    }

    [Fact]
    public void StringValueFromBlockString()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = \"\"\"word\"\"\") on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        StringValueNode valueNode = argument.DefaultValue.IsType<StringValueNode>();
        Assert.Equal("word", valueNode.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BooleanValue(bool value)
    {
        var t = new Core.TypeSystemParser($"directive @foo (fizz: buzz = {value.ToString().ToLower()}) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        BooleanValueNode valueNode = argument.DefaultValue.IsType<BooleanValueNode>();
        Assert.Equal(value, valueNode.Value);
    }

    [Fact]
    public void NullValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = null) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        argument.DefaultValue.IsType<NullValueNode>();
    }

    [Fact]
    public void EnumValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = ORANGE) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        EnumValueNode valueNode = argument.DefaultValue.IsType<EnumValueNode>();
        Assert.Equal("ORANGE", valueNode.Value);
    }

    [Fact]
    public void ListValueEmpty()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = []) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ListValueNode valueNode = argument.DefaultValue.IsType<ListValueNode>();
        valueNode.Values.NotNull().Count(0);
    }

    [Fact]
    public void ListValueOneEntry()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = [3]) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ListValueNode valueNode = argument.DefaultValue.IsType<ListValueNode>();
        IntValueNode entryNode = valueNode.Values.NotNull().One().IsType<IntValueNode>();
        Assert.Equal("3", entryNode.Value);
    }

    [Theory]
    [InlineData("directive @foo (fizz: buzz = [3,true,null]) on ENUM")]
    [InlineData("directive @foo (fizz: buzz = [3 true null]) on ENUM")]
    [InlineData("directive @foo (fizz: buzz = [3, true null ]) on ENUM")]
    public void ListValueThreeEntries(string schema)
    {
        var t = new Core.TypeSystemParser(schema);
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ListValueNode valueNode = argument.DefaultValue.IsType<ListValueNode>();
        var valueNodeList = valueNode.Values.NotNull().Count(3);
        IntValueNode entryNode1 = valueNodeList[0].IsType<IntValueNode>();
        Assert.Equal("3", entryNode1.Value);
        BooleanValueNode entryNode2 = valueNodeList[1].IsType<BooleanValueNode>();
        Assert.True(entryNode2.Value);
        valueNodeList[2].IsType<NullValueNode>();
    }

    [Fact]
    public void ListValueListValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = [3 [4]]) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ListValueNode valueNode = argument.DefaultValue.IsType<ListValueNode>();
        var valueNodeList = valueNode.Values.NotNull().Count(2);
        IntValueNode entryNode1 = valueNodeList[0].IsType<IntValueNode>();
        Assert.Equal("3", entryNode1.Value);
        ListValueNode innerNode = valueNodeList[1].IsType<ListValueNode>();
        var innerNodeList = innerNode.Values.NotNull().Count(1);
        IntValueNode entryNode2 = innerNodeList[0].IsType<IntValueNode>();
        Assert.Equal("4", entryNode2.Value);
    }

    [Fact]
    public void ListValueObjectValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = [3 { hello: null }]) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ListValueNode valueNode = argument.DefaultValue.IsType<ListValueNode>();
        var valueNodeList = valueNode.Values.NotNull().Count(2);
        IntValueNode entryNode1 = valueNodeList[0].IsType<IntValueNode>();
        Assert.Equal("3", entryNode1.Value);
        ObjectFieldNode fieldNode = valueNodeList[1].IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("hello", fieldNode.Name);
        Assert.IsType<NullValueNode>(fieldNode.Value);
    }

    [Fact]
    public void ObjectValueEmpty()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = {}) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ObjectValueNode valueNode = argument.DefaultValue.IsType<ObjectValueNode>();
        valueNode.ObjectFields.NotNull().Count(0);
    }

    [Fact]
    public void ObjectValueOneEntry()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = { world: 42 }) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ObjectFieldNode fieldNode = argument.DefaultValue.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("world", fieldNode.Name);
        IntValueNode intNode = fieldNode.Value.IsType<IntValueNode>();
        Assert.Equal("42", intNode.Value);
    }

    [Fact]
    public void ObjectValueObjectValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = { world: { hello: 42 } }) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ObjectFieldNode fieldNode1 = argument.DefaultValue.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("world", fieldNode1.Name);
        ObjectFieldNode fieldNode2 = fieldNode1.Value.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("hello", fieldNode2.Name);
        IntValueNode intNode = fieldNode2.Value.IsType<IntValueNode>();
        Assert.Equal("42", intNode.Value);
    }

    [Fact]
    public void ObjectValueListValue()
    {
        var t = new Core.TypeSystemParser("directive @foo (fizz: buzz = { world: [42] }) on ENUM");
        var documentNode = t.Parse();

        var argument = documentNode.NotNull().Directives.NotNull().One().Arguments.NotNull().One();
        ObjectFieldNode fieldNode = argument.DefaultValue.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("world", fieldNode.Name);
        IntValueNode entryNode = fieldNode.Value.IsType<ListValueNode>().Values.NotNull().One().IsType<IntValueNode>();
        Assert.Equal("42", entryNode.Value);
    }

    [Theory]
    [InlineData("directive @foo (fizz: buzz = { world: $ }) on ENUM", DocumentTokenKind.Dollar)]
    [InlineData("directive @foo (fizz: buzz = { world: @ }) on ENUM", DocumentTokenKind.At)]
    [InlineData("directive @foo (fizz: buzz = { world: : }) on ENUM", DocumentTokenKind.Colon)]
    [InlineData("directive @foo (fizz: buzz = { world: ... }) on ENUM", DocumentTokenKind.Spread)]
    public void TokenNotAllowedHere(string text, DocumentTokenKind tokenKind)
    {
        var t = new Core.TypeSystemParser(text);
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

