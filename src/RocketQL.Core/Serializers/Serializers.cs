namespace RocketQL.Core.Serializers;

public static class Serialization
{
    public static ValueNode JsonDeserialize(string source, string json)
    {
        return new JsonDeserializer(source, json).Deserialize();
    }

    public static string JsonSerialize(ValueNode node, bool format = false, int indent = 4)
    {
        return new JsonSerializer(node, format, indent).Serialize();
    }

    public static RequestNode RequestDeserialize(string source, string text)
    {
        return new RequestDeserializer(source, text).Deserialize();
    }

    public static SchemaNode SchemaDeserialize(string source, string text)
    {
        return new SchemaDeserializer(source, text).Deserialize();
    }

    public static SchemaNode SchemaDeserialize(string source, SchemaNode schema, string text)
    {
        return new SchemaDeserializer(source, schema, text).Deserialize();
    }
}
