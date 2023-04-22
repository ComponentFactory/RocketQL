namespace RocketQL.Core.Serializers;

public static class Document
{
    public static RequestNode RequestDeserialize(string text)
    {
        return new RequestDeserializer(text).Deserialize();
    }

    public static SchemaNode SchemaDeserialize(string text)
    {
        return new SchemaDeserializer(text).Deserialize();
    }
}
