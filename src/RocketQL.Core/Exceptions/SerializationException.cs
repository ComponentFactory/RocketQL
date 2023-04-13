namespace RocketQL.Core;

public class SerializationException : Exception
{
    public SerializationException(string message)
        : base(message)
    {
    }

    public static SerializationException CannotDeserializeCheckFormat() => new("Cannot deserialize JSON, check the JSON is formatted correctly.");
}


