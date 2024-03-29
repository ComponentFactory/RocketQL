﻿namespace RocketQL.Core.Serializers;

public ref struct JsonSerializer(ValueNode node, bool format = false, int indent = 4)
{
    private static readonly ThreadLocal<StringBuilder> s_cachedBuilder = new(() => new(4096));
    private readonly StringBuilder _sb = s_cachedBuilder.Value!;
    private int _depth = 0;

    public string Serialize()
    {
        _sb.Clear();

        if (!format)
            AppendNode(node);
        else
            AppendNodeFormat(node);

        return _sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendNode(ValueNode node)
    {
        switch (node)
        {
            case NullValueNode:
                _sb.Append("null");
                break;
            case BooleanValueNode boolNode:
                _sb.Append(boolNode.Value ? "true" : "false");
                break;
            case IntValueNode intNode:
                _sb.Append(intNode.Value);
                break;
            case FloatValueNode floatNode:
                _sb.Append(floatNode.Value);
                break;
            case StringValueNode stringNode:
                _sb.Append('\"');
                _sb.Append(stringNode.Value);
                _sb.Append('\"');
                break;
            case ListValueNode listNode:
                AppendListNode(listNode);
                break;
            case ObjectValueNode objectNode:
                AppendObjectNode(objectNode);
                break;
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendNodeFormat(ValueNode node)
    {
        switch (node)
        {
            case NullValueNode:
                _sb.Append("null");
                break;
            case BooleanValueNode boolNode:
                _sb.Append(boolNode.Value ? "true" : "false");
                break;
            case IntValueNode intNode:
                _sb.Append(intNode.Value);
                break;
            case FloatValueNode floatNode:
                _sb.Append(floatNode.Value);
                break;
            case StringValueNode stringNode:
                _sb.Append('\"');
                _sb.Append(stringNode.Value);
                _sb.Append('\"');
                break;
            case ListValueNode listNode:
                AppendListNodeFormat(listNode);
                break;
            case ObjectValueNode objectNode:
                AppendObjectNodeFormat(objectNode);
                break;
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendListNode(ListValueNode listNode)
    {
        _sb.Append('[');

        var first = true;
        foreach (var node in listNode.Values)
        {
            if (first)
                first = false;
            else
                _sb.Append(',');

            AppendNode(node);
        }

        _sb.Append(']');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendListNodeFormat(ListValueNode listNode)
    {
        _sb.Append('[');
        _depth++;

        var first = true;
        foreach (var node in listNode.Values)
        {
            if (first)
                first = false;
            else
                _sb.Append(',');

            _sb.AppendLine();
            _sb.Append(' ', _depth * indent);
            AppendNodeFormat(node);
        }

        _sb.AppendLine();
        _sb.Append(' ', --_depth * indent);
        _sb.Append(']');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendObjectNode(ObjectValueNode objNode)
    {
        _sb.Append('{');

        var first = true;
        foreach (var node in objNode.ObjectFields)
        {
            if (first)
                first = false;
            else
                _sb.Append(',');

            _sb.Append('\"');
            _sb.Append(node.Name);
            _sb.Append("\":");
            AppendNode(node.Value);
        }

        _sb.Append('}');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendObjectNodeFormat(ObjectValueNode objNode)
    {
        _sb.Append('{');
        _depth++;

        var first = true;
        foreach (var node in objNode.ObjectFields)
        {
            if (first)
                first = false;
            else
                _sb.Append(',');

            _sb.AppendLine();
            _sb.Append(' ', _depth * indent);
            _sb.Append('\"');
            _sb.Append(node.Name);
            _sb.Append("\": ");
            AppendNodeFormat(node.Value);
        }

        _sb.AppendLine();
        _sb.Append(' ', --_depth * indent);
        _sb.Append('}');
    }
}
