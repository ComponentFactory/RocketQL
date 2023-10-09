namespace RocketQL.Core.Nodes;

public abstract record class ValueNode();

public class ValueNodeList : List<ValueNode> { };

public record class NullValueNode() : ValueNode()
{
    public static readonly NullValueNode Null = new();
}

public record class BooleanValueNode(bool Value) : ValueNode()
{
    public static readonly BooleanValueNode True = new(true);
    public static readonly BooleanValueNode False = new(false);
}

public record class IntValueNode(string Value) : ValueNode();
public record class FloatValueNode(string Value) : ValueNode();
public record class StringValueNode(string Value) : ValueNode();
public record class EnumValueNode(string Value) : ValueNode();
public record class ObjectFieldNode(string Name, ValueNode Value) : ValueNode();
public record class VariableValueNode(string Value) : ValueNode();

public record class ListValueNode(ValueNodeList Values) : ValueNode();
public record class ObjectValueNode(ObjectFieldNodeList ObjectFields) : ValueNode();
