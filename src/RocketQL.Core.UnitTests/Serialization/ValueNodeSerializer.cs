using RocketQL.Core.UnitTests.GraphQLParser;

namespace RocketQL.Core.UnitTests.Serialization;

public class ValueNodeSerializer
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrWhitespace(string json)
    {
        Assert.Equal(NullValueNode.Null, new Core.ValueNodeSerializer().Deserialize(json));
    }

    [Fact]
    public void Null()
    {
        Assert.Equal(NullValueNode.Null, new Core.ValueNodeSerializer().Deserialize("null"));
    }

    [Fact]
    public void True()
    {
        Assert.Equal(BooleanValueNode.True, new Core.ValueNodeSerializer().Deserialize("true"));
    }

    [Fact]
    public void False()
    {
        Assert.Equal(BooleanValueNode.False, new Core.ValueNodeSerializer().Deserialize("false"));
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"hel\\tlo\"", "hel\tlo")]
    public void String(string json, string value)
    {
        Assert.Equal(new StringValueNode(value), new Core.ValueNodeSerializer().Deserialize(json));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("42")]
    [InlineData("-42")]
    public void IntegerNumber(string json)
    {
        Assert.Equal(new IntValueNode(json), new Core.ValueNodeSerializer().Deserialize(json));
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("0.42")]
    [InlineData("0e1")]
    public void FloatNumber(string json)
    {
        Assert.Equal(new FloatValueNode(json), new Core.ValueNodeSerializer().Deserialize(json));
    }

    [Theory]
    [InlineData("[]", 0)]
    [InlineData("[1]", 1)]
    [InlineData("[1,2]", 2)]
    [InlineData("[1,2,3,4,5]", 5)]
    public void ListOfNumbers(string json, int count)
    {
        ListValueNode list = (ListValueNode)new Core.ValueNodeSerializer().Deserialize(json);
        list.Values.NotNull().Count(count);
    }

    [Fact]
    public void EmptyObject()
    {
        ObjectValueNode obj = (ObjectValueNode)new Core.ValueNodeSerializer().Deserialize("{}");
        obj.ObjectFields.NotNull().Count(0);
    }


    [Fact]
    public void ObjectOneField()
    {
        ObjectValueNode obj = (ObjectValueNode)new Core.ValueNodeSerializer().Deserialize("{ \"a\": 1 }");
        obj.ObjectFields.NotNull().Count(1);
        Assert.Equal("a", obj.ObjectFields[0].Name);
        Assert.Equal(new IntValueNode("1"), obj.ObjectFields[0].Value);
    }

    [Fact]
    public void ObjectMultipleField()
    {
        ObjectValueNode obj = (ObjectValueNode)new Core.ValueNodeSerializer().Deserialize("{ \"a\": 1, \"b\": true, \"c\": \"foo\" }");
        obj.ObjectFields.NotNull().Count(3);
        Assert.Equal("a", obj.ObjectFields[0].Name);
        Assert.Equal(new IntValueNode("1"), obj.ObjectFields[0].Value);
        Assert.Equal("b", obj.ObjectFields[1].Name);
        Assert.Equal(BooleanValueNode.True, obj.ObjectFields[1].Value);
        Assert.Equal("c", obj.ObjectFields[2].Name);
        Assert.Equal(new StringValueNode("foo"), obj.ObjectFields[2].Value);
    }

    [Fact]
    public void ListInsideObject()
    {
        ObjectValueNode obj = (ObjectValueNode)new Core.ValueNodeSerializer().Deserialize("{ \"a\": [1, 2, 3], \"b\": true }");
        obj.ObjectFields.NotNull().Count(2);
        Assert.Equal("a", obj.ObjectFields[0].Name);
        ObjectFieldNode field = obj.ObjectFields[0].IsType<ObjectFieldNode>();
        ListValueNode list = field.Value.IsType<ListValueNode>();
        list.Values.NotNull().Count(3);
        var num1 = list.Values[0].IsType<IntValueNode>();
        var num2 = list.Values[1].IsType<IntValueNode>();
        var num3 = list.Values[2].IsType<IntValueNode>();
        Assert.Equal("1", num1.Value);
        Assert.Equal("2", num2.Value);
        Assert.Equal("3", num3.Value);
        Assert.Equal("b", obj.ObjectFields[1].Name);
        Assert.Equal(BooleanValueNode.True, obj.ObjectFields[1].Value);
    }

    [Fact]
    public void ObjectInsideList()
    {
        ListValueNode list = (ListValueNode)new Core.ValueNodeSerializer().Deserialize("[ { \"a\": 1 }, 2 ]");
        list.Values.NotNull().Count(2);
        ObjectValueNode obj = list.Values[0].IsType<ObjectValueNode>();
        obj.ObjectFields.NotNull().Count(1);
        Assert.Equal("a", obj.ObjectFields[0].Name);
        var num1 = obj.ObjectFields[0].Value.IsType<IntValueNode>();
        Assert.Equal("1", num1.Value);
        IntValueNode num2 = list.Values[1].IsType<IntValueNode>();
        Assert.Equal("2", num2.Value);
    }
}

