namespace RocketQL.Core.UnitTests.JsonDeserialize;

public class Value : UnitTestBase
{

    [Fact]
    public void NullValue()
    {
        var rootNode = Serialization.JsonDeserialize("null");

        rootNode.IsType<NullValueNode>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BooleanValue(bool value)
    {
        var rootNode = Serialization.JsonDeserialize($"{value.ToString().ToLower()}");

        BooleanValueNode valueNode = rootNode.IsType<BooleanValueNode>();
        Assert.Equal(value, valueNode.Value);
    }


    [Fact]
    public void IntValue()
    {
        var rootNode = Serialization.JsonDeserialize("1");

        IntValueNode valueNode = rootNode.IsType<IntValueNode>();
        Assert.Equal("1", valueNode.Value);
    }

    [Fact]
    public void FloatValue()
    {
        var rootNode = Serialization.JsonDeserialize("3.14159");

        FloatValueNode valueNode = rootNode.IsType<FloatValueNode>();
        Assert.Equal("3.14159", valueNode.Value);
    }

    [Fact]
    public void StringValue()
    {
        var rootNode = Serialization.JsonDeserialize("\"word\"");

        StringValueNode valueNode = rootNode.IsType<StringValueNode>();
        Assert.Equal("word", valueNode.Value);
    }


    [Fact]
    public void ListValueEmpty()
    {
        var rootNode = Serialization.JsonDeserialize("[]");

        ListValueNode valueNode = rootNode.IsType<ListValueNode>();
        valueNode.Values.NotNull().Count(0);
    }

    [Fact]
    public void ListValueOneEntry()
    {
        var rootNode = Serialization.JsonDeserialize("[3]");

        ListValueNode valueNode = rootNode.IsType<ListValueNode>();
        IntValueNode entryNode = valueNode.Values.NotNull().One().IsType<IntValueNode>();
        Assert.Equal("3", entryNode.Value);
    }

    [Theory]
    [InlineData("[3,true,null]")]
    [InlineData("[3 true, null]")]
    [InlineData("[3, true, null ]")]
    public void ListValueThreeEntries(string schema)
    {
        var rootNode = Serialization.JsonDeserialize(schema);

        ListValueNode valueNode = rootNode.IsType<ListValueNode>();
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
        var rootNode = Serialization.JsonDeserialize("[3, [4]]");

        ListValueNode valueNode = rootNode.IsType<ListValueNode>();
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
        var rootNode = Serialization.JsonDeserialize("[3, { \"hello\": null }]");

        ListValueNode valueNode = rootNode.IsType<ListValueNode>();
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
        var rootNode = Serialization.JsonDeserialize("{}");

        ObjectValueNode valueNode = rootNode.IsType<ObjectValueNode>();
        valueNode.ObjectFields.NotNull().Count(0);
    }

    [Fact]
    public void ObjectValueOneEntry()
    {
        var rootNode = Serialization.JsonDeserialize("{ \"world\": 42 }");

        ObjectFieldNode fieldNode = rootNode.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("world", fieldNode.Name);
        IntValueNode intNode = fieldNode.Value.IsType<IntValueNode>();
        Assert.Equal("42", intNode.Value);
    }

    [Fact]
    public void ObjectValueObjectValue()
    {
        var rootNode = Serialization.JsonDeserialize("{ \"world\": { \"hello\": 42 } }");

        ObjectFieldNode fieldNode1 = rootNode.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("world", fieldNode1.Name);
        ObjectFieldNode fieldNode2 = fieldNode1.Value.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("hello", fieldNode2.Name);
        IntValueNode intNode = fieldNode2.Value.IsType<IntValueNode>();
        Assert.Equal("42", intNode.Value);
    }

    [Fact]
    public void ObjectValueListValue()
    {
        var rootNode = Serialization.JsonDeserialize("{ \"world\": [42] }");

        ObjectFieldNode fieldNode = rootNode.IsType<ObjectValueNode>().ObjectFields.NotNull().One();
        Assert.Equal("world", fieldNode.Name);
        IntValueNode entryNode = fieldNode.Value.IsType<ListValueNode>().Values.NotNull().One().IsType<IntValueNode>();
        Assert.Equal("42", entryNode.Value);
    }

    [Theory]
    [InlineData("{ \"world\": } }", JsonTokenKind.RightCurlyBracket)]
    [InlineData("{ \"world\": : }", JsonTokenKind.Colon)]
    public void TokenNotAllowedHere(string text, JsonTokenKind tokenKind)
    {
        try
        {
            var rootNode = Serialization.JsonDeserialize(text);
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

