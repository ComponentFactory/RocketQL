namespace RocketQL.Core.Serializers;

public ref struct JsonDeserializer

{
    private JsonTokenizer _tokenizer;

    public JsonDeserializer(ReadOnlySpan<char> json)
    {
        _tokenizer = new JsonTokenizer(json);
    }

    public ValueNode Deserialize()
    {
        // Move to the first real token
        _tokenizer.Next();
        return ParseValue();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseValue()
    {
        return _tokenizer.TokenKind switch
        {
            JsonTokenKind.TrueValue => BooleanValueNode.True,
            JsonTokenKind.FalseValue => BooleanValueNode.False,
            JsonTokenKind.NullValue => NullValueNode.Null,
            JsonTokenKind.StringValue => new StringValueNode(_tokenizer.TokenString),
            JsonTokenKind.IntValue => new IntValueNode(_tokenizer.TokenValue),
            JsonTokenKind.FloatValue => new FloatValueNode(_tokenizer.TokenValue),
            JsonTokenKind.LeftSquareBracket => ParseArray(),
            JsonTokenKind.LeftCurlyBracket => ParseObject(),
            _ => throw SyntaxException.TokenNotAllowedHere(_tokenizer.Location, _tokenizer.TokenKind.ToString())
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseArray()
    {
        MandatoryNext();

        ValueNodeList values = new();

        while (_tokenizer.TokenKind != JsonTokenKind.RightSquareBracket)
        {
            values.Add(ParseValue());
            MandatoryNext();

            if (_tokenizer.TokenKind == JsonTokenKind.Comma)
                MandatoryNext();
        }

        return new ListValueNode(values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseObject()
    {
        MandatoryNext();

        ObjectFieldNodeList values = new();

        while (_tokenizer.TokenKind != JsonTokenKind.RightCurlyBracket)
        {
            MandatoryToken(JsonTokenKind.StringValue);
            string propertyName = _tokenizer.TokenString;
            MandatoryNextToken(JsonTokenKind.Colon);
            MandatoryNext();
            values.Add(new ObjectFieldNode(propertyName, ParseValue()));
            MandatoryNext();

            if (_tokenizer.TokenKind == JsonTokenKind.Comma)
                MandatoryNext();
        }

        return new ObjectValueNode(values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNext()
    {
        if (_tokenizer.TokenKind == JsonTokenKind.EndOfText || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryToken(JsonTokenKind token)
    {
        if (_tokenizer.TokenKind == JsonTokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token.ToString(), _tokenizer.TokenKind.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNextToken(JsonTokenKind token)
    {
        if (_tokenizer.TokenKind == JsonTokenKind.EndOfText || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token.ToString(), _tokenizer.TokenKind.ToString());
    }
}
