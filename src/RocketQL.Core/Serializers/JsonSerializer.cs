namespace RocketQL.Core.Serializers;

public ref struct JsonSerializer
{
    private static readonly ThreadLocal<StringBuilder> _cachedBuilder = new(() => new(4096));

    private readonly ValueNode _node;
    private readonly StringBuilder _sb;
    private readonly bool _format;
    private int _indent;
    private int _depth = 0;

    public JsonSerializer(ValueNode node, bool format = false, int indent = 4)
    {
        _node = node;
        _sb = _cachedBuilder.Value!;
        _format = format;
        _indent = indent;
    }

    public string Serialize()
    {
        _sb.Clear();

        if (!_format)
            AppendNode(_node);
        else
            AppendNodeFormat(_node);

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
        switch(node)
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

        bool first = true;
        foreach (ValueNode node in listNode.Values)
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

        bool first = true;
        foreach (ValueNode node in listNode.Values)
        {
            if (first)
                first = false;
            else
                _sb.Append(',');

            _sb.AppendLine();
            _sb.Append(' ', _depth * _indent);
            AppendNodeFormat(node);
        }

        _sb.AppendLine();
        _sb.Append(' ', --_depth * _indent);
        _sb.Append(']');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendObjectNode(ObjectValueNode objNode)
    {
        _sb.Append('{');

        bool first = true;
        foreach (ObjectFieldNode node in objNode.ObjectFields)
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

        bool first = true;
        foreach (ObjectFieldNode node in objNode.ObjectFields)
        {
            if (first)
                first = false;
            else
                _sb.Append(',');

            _sb.AppendLine();
            _sb.Append(' ', _depth * _indent);
            _sb.Append('\"');
            _sb.Append(node.Name);
            _sb.Append("\": ");
            AppendNodeFormat(node.Value);
        }

        _sb.AppendLine();
        _sb.Append(' ', --_depth * _indent);
        _sb.Append('}');
    }
}
