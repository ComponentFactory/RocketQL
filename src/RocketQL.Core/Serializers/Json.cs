namespace RocketQL.Core.Serializers;

public static class Json
{
    public static ValueNode Deserialize(string json)
    {
        return new JsonDeserializer(json).Deserialize();
    }

    public static string Serialize(ValueNode node, bool format = false, int indent = 4)
    {
        return new JsonSerializer(node, format, indent).Serialize();
    }
}
