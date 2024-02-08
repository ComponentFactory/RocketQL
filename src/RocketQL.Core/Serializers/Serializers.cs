namespace RocketQL.Core.Serializers;

public static class Serialization
{
    public static string JsonSerialize(ValueNode node, bool format = false, int indent = 4)
    {
        return new JsonSerializer(node, format, indent).Serialize();
    }

    public static ValueNode JsonDeserialize(ReadOnlySpan<char> json, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new JsonDeserializer(json, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)).Deserialize();
    }

    public static ValueNode JsonDeserialize(ReadOnlySpan<char> json, string source)
    {
        return new JsonDeserializer(json, source).Deserialize();
    }

    public static SyntaxNodeList RequestDeserialize(ReadOnlySpan<char> text, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new RequestDeserializer(text, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)).Deserialize();
    }

    public static SyntaxNodeList RequestDeserialize(ReadOnlySpan<char> text, string source)
    {
        return new RequestDeserializer(text, source).Deserialize();
    }

    public static string SchemaSerialize(ISchema schema, SchemaSerializeOptions? options = null)
    {
        return new SchemaSerializer(schema).Serialize(options);
    }

    public static SyntaxNodeList SchemaDeserialize(ReadOnlySpan<char> text, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new SchemaDeserializer(text, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)).Deserialize();
    }

    public static SyntaxNodeList SchemaDeserialize(ReadOnlySpan<char> text, string source)
    {
        return new SchemaDeserializer(text, source).Deserialize();
    }
}
