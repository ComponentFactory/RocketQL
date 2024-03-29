﻿namespace RocketQL.Core.UnitTests.JsonSerializer;

public class Value : UnitTestBase
{
    [Fact]
    public void NullValue()
    {
        var t1 = Serialization.JsonSerialize(NullValueNode.Null);
        var t2 = Serialization.JsonSerialize(new NullValueNode());

        Assert.Equal("null", t1);
        Assert.Equal("null", t2);
    }

    [Fact]
    public void BooleanValue()
    {
        var t1 = Serialization.JsonSerialize(BooleanValueNode.True);
        var t2 = Serialization.JsonSerialize(BooleanValueNode.False);
        var t3 = Serialization.JsonSerialize(new BooleanValueNode(true));
        var t4 = Serialization.JsonSerialize(new BooleanValueNode(false));

        Assert.Equal("true", t1);
        Assert.Equal("false", t2);
        Assert.Equal("true", t3);
        Assert.Equal("false", t4);
    }

    [Fact]
    public void IntValue()
    {
        var t = Serialization.JsonSerialize(new IntValueNode("42"));

        Assert.Equal("42", t);
    }

    [Fact]
    public void FloatValue()
    {
        var t = Serialization.JsonSerialize(new FloatValueNode("3.14"));

        Assert.Equal("3.14", t);
    }

    [Fact]
    public void StringValue()
    {
        var t = Serialization.JsonSerialize(new StringValueNode("word"));

        Assert.Equal("\"word\"", t);
    }

    [Theory]
    [InlineData("[]", "[]")]
    [InlineData("[3]", "[3]")]
    [InlineData("[3,true,null]", "[3,true,null]")]
    [InlineData("[3, [4.9]]", "[3,[4.9]]")]
    public void ListValue(string before, string after)
    {
        var node = Serialization.JsonDeserialize(before);
        var json = Serialization.JsonSerialize(node);

        Assert.Equal(after, json);
    }

    [Theory]
    [InlineData("{}", "{}")]
    [InlineData("{\"world\": 42}", "{\"world\":42}")]
    [InlineData("{\"world\": 42, \"hello\": true}", "{\"world\":42,\"hello\":true}")]
    [InlineData("{\"world\": { \"hello\": 42}}", "{\"world\":{\"hello\":42}}")]
    public void ObjectValue(string before, string after)
    {
        var node = Serialization.JsonDeserialize(before);
        var json = Serialization.JsonSerialize(node);

        Assert.Equal(after, json);
    }

    [Theory]
    [InlineData("[{\"world\": 42}]", "[{\"world\":42}]")]
    [InlineData("{\"world\": [1, 2, 3]}", "{\"world\":[1,2,3]}")]
    public void ListValueAndObjectValue(string before, string after)
    {
        var node = Serialization.JsonDeserialize(before);
        var json = Serialization.JsonSerialize(node);

        Assert.Equal(after, json);
    }

    [Theory]
    [InlineData("[]", """
        [
        ]
        """)]
    [InlineData("[3]", """
        [
            3
        ]
        """)]
    [InlineData("[3,4]", """
        [
            3,
            4
        ]
        """)]
    [InlineData("[3, [4.9]]", """
        [
            3,
            [
                4.9
            ]
        ]
        """)]
    public void ListValueFormat(string before, string after)
    {
        var node = Serialization.JsonDeserialize(before);
        var json = Serialization.JsonSerialize(node, true);

        Assert.Equal(after, json);
    }

    [Theory]
    [InlineData("{}", """
        {
        }
        """)]
    [InlineData("{\"world\": 42}", """
        {
            "world": 42
        }
        """)]
    [InlineData("{\"world\": 42, \"hello\": true}", """
        {
            "world": 42,
            "hello": true
        }
        """)]
    [InlineData("{\"world\": { \"hello\": 42}}", """
        {
            "world": {
                "hello": 42
            }
        }
        """)]
    public void ObjectValueFormat(string before, string after)
    {
        var node = Serialization.JsonDeserialize(before);
        var json = Serialization.JsonSerialize(node, true);

        Assert.Equal(after, json);
    }

    [Theory]
    [InlineData("[{\"world\": 42}]", """
        [
            {
                "world": 42
            }
        ]
        """)]
    [InlineData("{\"world\": [1, 2, 3]}", """
        {
            "world": [
                1,
                2,
                3
            ]
        }
        """)]
    [InlineData("""
        {
            "int": 123,
            "float": 3.14,
            "string": "hello",
            "listints": [1,2,3],
            "object": {
                "int": 123,
                "float": 3.14,
                "string": "hello",
                "listints": [
                    1,2,3
                ],
                "child": {
                    "int": 123,
                    "float": 3.14,
                    "string": "hello",
                    "listints": [1, 2, 3],
                    "object": {
                        "int": 123,
                        "float": 3.14,
                        "string": "hello",
                        "listints": [
                            1,
                            2,
                            3
                        ]
                    }
                }
            },
            "listobj": [null, true, false]
        }
        """,
        """
        {
            "int": 123,
            "float": 3.14,
            "string": "hello",
            "listints": [
                1,
                2,
                3
            ],
            "object": {
                "int": 123,
                "float": 3.14,
                "string": "hello",
                "listints": [
                    1,
                    2,
                    3
                ],
                "child": {
                    "int": 123,
                    "float": 3.14,
                    "string": "hello",
                    "listints": [
                        1,
                        2,
                        3
                    ],
                    "object": {
                        "int": 123,
                        "float": 3.14,
                        "string": "hello",
                        "listints": [
                            1,
                            2,
                            3
                        ]
                    }
                }
            },
            "listobj": [
                null,
                true,
                false
            ]
        }
        """)]
    public void ListValueAndObjectValueFormat(string before, string after)
    {
        var node = Serialization.JsonDeserialize(before);
        var json = Serialization.JsonSerialize(node, true);

        Assert.Equal(after, json);
    }
}

