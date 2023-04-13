using System.Buffers;

namespace RocketQL.Core;

public ref struct ValueNodeSerializer
{
    private const int MaxExpansionFactor = 3;
    private const long ArrayPoolMaxSize = 1024 * 1024;
    private const long MaxLengthBeforeNormalAlloc = ArrayPoolMaxSize / MaxExpansionFactor;

    private static readonly byte[] _floatChars = new byte[] { 69, 101, 46 }; // e, E, .

    private static readonly JsonReaderOptions _defaultOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private Utf8JsonReader _reader;

    public ValueNode Deserialize(string text)
    {
        return Deserialize(text, _defaultOptions);
    }

    public ValueNode Deserialize(string json, JsonReaderOptions options)
    {
        if (string.IsNullOrEmpty(json))
            return NullValueNode.Null;

        byte[]? tempArray = null;

        Span<byte> jsonUtf8Bytes = json.Length <= MaxLengthBeforeNormalAlloc ?
            tempArray = ArrayPool<byte>.Shared.Rent(json.Length * MaxExpansionFactor) :
            new byte[Encoding.UTF8.GetByteCount(json)];

        try
        {
            _reader = new Utf8JsonReader(jsonUtf8Bytes[..Encoding.UTF8.GetBytes(json, jsonUtf8Bytes)], options);

            if (!_reader.Read())
                throw SerializationException.CannotDeserializeCheckFormat();

            return Parse();
        }
        catch
        {
            throw SerializationException.CannotDeserializeCheckFormat();
        }
        finally
        {
            if (tempArray != null)
            {
                jsonUtf8Bytes.Clear();
                ArrayPool<byte>.Shared.Return(tempArray);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode Parse()
    {
        return _reader.TokenType switch
        {
            JsonTokenType.True => BooleanValueNode.True,
            JsonTokenType.False => BooleanValueNode.False,
            JsonTokenType.Null => NullValueNode.Null,
            JsonTokenType.None => NullValueNode.Null,
            JsonTokenType.String => new StringValueNode(_reader.GetString()!),
            JsonTokenType.Number => ParseNumber(),
            JsonTokenType.StartArray => ParseArray(),
            JsonTokenType.StartObject => ParseObject(),
             _ => throw SerializationException.CannotDeserializeCheckFormat()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseNumber()
    {
        var span = _reader.HasValueSequence ? _reader.ValueSequence.ToArray() : _reader.ValueSpan;
        if (span.IndexOfAny(_floatChars.AsSpan()) >= 0)
            return new FloatValueNode(Encoding.UTF8.GetString(span));
        else
            return new IntValueNode(Encoding.UTF8.GetString(span));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseArray()
    {
        ValueNodeList values = new();

        while (_reader.Read() && _reader.TokenType != JsonTokenType.EndArray)
            values.Add(Parse());

        return new ListValueNode(values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseObject()
    {
        ObjectFieldNodeList values = new();

        while (_reader.Read() && _reader.TokenType != JsonTokenType.EndObject)
        {
            string propertyName = _reader.GetString()!;
            _reader.Read();
            values.Add(new ObjectFieldNode(propertyName, Parse()));
        }

        return new ObjectValueNode(values);
    }
}
